namespace MadWorldEU.Byakko.Connections;

/// <summary>Configuration options for the SMTP mail service.</summary>
public sealed class MailOptions
{
    public const string SectionName = "Mail";
    
    /// <summary>Mail provider mode (e.g. <c>Mailpit</c>, <c>Smtp</c>).</summary>
    public string Mode { get; init; } = string.Empty;
    /// <summary>SMTP server hostname.</summary>
    public string Host { get; set; } = string.Empty;
    /// <summary>SMTP server port.</summary>
    public int Port { get; set; }
    /// <summary>SMTP authentication username.</summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>SMTP authentication token or password.</summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>Enables TLS/SSL encryption for the SMTP connection.</summary>
    public bool TlsEnabled { get; set; }
    /// <summary>Sender address used on outgoing administrator emails.</summary>
    public MailAddressOptions AdministratorFrom { get; init; } = new();
    /// <summary>Recipient address for outgoing administrator emails.</summary>
    public MailAddressOptions AdministratorTo { get; init; } = new();
    
    public bool HasAuthentication => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Token);
}