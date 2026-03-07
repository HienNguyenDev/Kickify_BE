using Kickify.Application.Abstractions.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Jobs;

/// <summary>
/// Hosted service ch?y khi app start ?? ??ng ký recurring jobs
/// và populate initial cache
/// </summary>
public class JobSchedulerStartupService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<JobSchedulerStartupService> _logger;

    public JobSchedulerStartupService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<JobSchedulerStartupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering recurring jobs...");

        using var scope = _serviceScopeFactory.CreateScope();
        var leaderboardUpdateService = scope.ServiceProvider.GetRequiredService<ILeaderboardUpdateService>();

        // ??ng ký recurring job ch?y m?i ngày 00:00 UTC
        leaderboardUpdateService.ScheduleLeaderboardUpdate();

        // Populate cache ngay l?n ??u n?u ch?a có
        await leaderboardUpdateService.UpdateLeaderboardCacheAsync();

        _logger.LogInformation("Recurring jobs registered successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
