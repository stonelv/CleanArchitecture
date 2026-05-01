using Microsoft.Extensions.Options;
using MinimalClean.Architecture.Web.Configurations;
using MinimalClean.Architecture.Web.Domain.Interfaces;

namespace MinimalClean.Architecture.Web.Infrastructure.Data.Services;

public class AuditLogCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<AuditLogCleanupOptions> _options;
    private readonly ILogger<AuditLogCleanupBackgroundService> _logger;

    public AuditLogCleanupBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<AuditLogCleanupOptions> options,
        ILogger<AuditLogCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.EnableAutoCleanup)
        {
            _logger.LogInformation("Audit log auto-cleanup is disabled");
            return;
        }

        _logger.LogInformation(
            "Audit log auto-cleanup started. Retention days: {RetentionDays}, Schedule: {Schedule}",
            _options.Value.RetentionDays,
            _options.Value.CleanupSchedule);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateNextRunDelay();
            _logger.LogInformation("Next audit log cleanup scheduled in {Delay}", delay);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Starting audit log cleanup...");
                
                using var scope = _serviceProvider.CreateScope();
                var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                
                var result = await auditLogService.CleanupAsync(
                    _options.Value.RetentionDays,
                    stoppingToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Audit log cleanup completed successfully. Deleted {Count} logs older than {Days} days",
                        result.Value,
                        _options.Value.RetentionDays);
                }
                else
                {
                    _logger.LogError(
                        "Audit log cleanup failed: {Errors}",
                        string.Join(", ", result.Errors));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Audit log cleanup was canceled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit log cleanup");
            }
        }

        _logger.LogInformation("Audit log auto-cleanup stopped");
    }

    private TimeSpan CalculateNextRunDelay()
    {
        var now = DateTime.Now;
        var nextRun = now.Date.AddDays(1).AddHours(0);
        
        var delay = nextRun - now;
        
        if (delay <= TimeSpan.Zero)
        {
            delay = TimeSpan.FromHours(1);
        }

        return delay;
    }
}
