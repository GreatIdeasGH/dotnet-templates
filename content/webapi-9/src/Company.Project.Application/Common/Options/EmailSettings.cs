namespace Company.Project.Application.Common.Options;

public struct EmailSettings
{
    public string SmtpHost { get; set; }
    public string SmtpPort { get; set; }
    public string FromAddress { get; set; }
    public string FromName { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPassword { get; set; }
    public string CcName { get; set; }
    public string CcAddress { get; set; }
    public string BusinessNameTag { get; set; }
    public string TeamNameTag { get; set; }
    public string Website { get; set; }
    public string Domain { get; set; }
    public string LogicAppUrl { get; set; }

    /// <summary>
    /// Timeout in seconds for SMTP operations. Default is 30 seconds.
    /// </summary>
    public int TimeoutInSeconds { get; set; } //= 30;

    /// <summary>
    /// Maximum number of retry attempts for transient failures. Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } //= 3;
}
