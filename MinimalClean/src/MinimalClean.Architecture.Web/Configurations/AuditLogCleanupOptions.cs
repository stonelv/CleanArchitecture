namespace MinimalClean.Architecture.Web.Configurations;

public class AuditLogCleanupOptions
{
    public const string ConfigurationSectionName = "AuditLogCleanup";

    public bool EnableAutoCleanup { get; set; } = true;

    public int RetentionDays { get; set; } = 30;

    public string CleanupSchedule { get; set; } = "0 0 * * *";

    public int BatchSize { get; set; } = 1000;
}
