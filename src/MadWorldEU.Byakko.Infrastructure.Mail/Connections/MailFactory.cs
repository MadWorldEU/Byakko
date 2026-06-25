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
            "OvhCloud" => options.OverrideByMailPit(configuration),
            _ => throw new InvalidOperationException($"Unknown mail mode: {options.Mode}")
        };
    }

    private static MailOptions OverrideByMailPit(this MailOptions options, IConfiguration configuration)
    {
        options.Host = configuration.GetValue<string>("MailPit:Host")!;
        options.Port = configuration.GetValue<int>("MailPit:Port");
        
        return options;
    }
}