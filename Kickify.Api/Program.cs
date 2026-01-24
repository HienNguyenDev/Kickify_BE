using HealthChecks.UI.Client;
using Kickify.Api;
using Kickify.Api.Extensions;
using Kickify.Api.Hubs;
using Kickify.Application;
using Kickify.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();
builder.Services
            .AddApplication()
            .AddPresentation(builder.Configuration, builder.Environment)
            .AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200 * 1024 * 1024; 
});

var app = builder.Build();

app.UseSwaggerWithUi();

app.ApplyMigrations();

app.MapHealthChecks("health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseCors("AllowAll");

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<MatchRoomHub>("/hubs/matchroom");

app.Run();
