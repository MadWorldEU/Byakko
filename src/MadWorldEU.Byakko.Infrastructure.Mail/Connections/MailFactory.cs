namespace MadWorldEU.Byakko.Connections;

public static class MailFactory
{
    public static MailOptions Create(IConfiguration configuration)
    {
        var options = new MailOptions();
        configuration.GetSection(MailOptions.SectionName).Bind(options);

        return options.Mode switch
        {
            "Smtp" => options,
            "MailPit" => options.OverrideByMailPit(configuration),
            _ => throw new InvalidOperationException($"Unknown mail mode: {options.Mode}")
        };
    }

    private static MailOptions OverrideByMailPit(this MailOptions options, IConfiguration configuration)
    {
        options.Host = configuration.GetValue<string>("MAILPIT_HOST") ?? throw new InvalidOperationException("MAILPIT_HOST is not configured.");
        options.Port = configuration.GetValue<int>("MAILPIT_PORT");
        options.Username = string.Empty;
        options.Token = string.Empty;
        options.TlsEnabled = false;
        
        return options;
    }
}