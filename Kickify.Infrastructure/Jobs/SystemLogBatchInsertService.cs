using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Services;
using Kickify.Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Jobs;

public sealed class SystemLogBatchInsertService(
    SystemLogQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<SystemLogBatchInsertService> logger) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ErrorBackoff = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<SystemLog>(BatchSize);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    while (buffer.Count < BatchSize && queue.Reader.TryRead(out var item))
                    {
                        buffer.Add(Map(item));
                    }

                    if (buffer.Count >= BatchSize)
                    {
                        await FlushAsync(buffer, stoppingToken);
                        continue;
                    }

                    // Do not use PeriodicTimer with Task.WhenAny: only one WaitForNextTickAsync may be
                    // in flight per timer; if the channel wins, the next loop would start a second wait
                    // and throw InvalidOperationException.
                    var waitForDataTask = queue.Reader.WaitToReadAsync(stoppingToken).AsTask();
                    var delayTask = Task.Delay(FlushInterval, stoppingToken);
                    await Task.WhenAny(waitForDataTask, delayTask);

                    if (buffer.Count > 0)
                    {
                        await FlushAsync(buffer, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "System log batch worker iteration failed. Retrying in {RetryDelaySeconds} seconds.", ErrorBackoff.TotalSeconds);
                    await Task.Delay(ErrorBackoff, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
        finally
        {
            if (buffer.Count > 0)
            {
                try
                {
                    await FlushAsync(buffer, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to flush remaining system logs on shutdown.");
                }
            }
        }
    }

    private async Task FlushAsync(List<SystemLog> logs, CancellationToken cancellationToken)
    {
        if (logs.Count == 0)
        {
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.SystemLogs.AddRangeAsync(logs, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logs.Clear();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to batch-insert {Count} system logs.", logs.Count);
            logs.Clear();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure while batch-inserting {Count} system logs.", logs.Count);
            logs.Clear();
        }
    }

    private static SystemLog Map(SystemLogQueueItem item)
    {
        return new SystemLog
        {
            LogId = Guid.NewGuid(),
            UserId = item.UserId,
            UserName = item.UserName,
            Action = item.Action,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            UserAgent = item.UserAgent,
            ResponseStatus = item.ResponseStatus,
            ErrorMessage = item.ErrorMessage,
            CreatedAt = item.CreatedAtUtc
        };
    }
}
