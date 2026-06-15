namespace MadWorldEU.Byakko.Audits;

public sealed class IpAddress : ValueObject
{
    public string Value { get; }

    private IpAddress(string ipAddress)
    {
        Value = ipAddress;   
    }
    
    public static Result<IpAddress> Create(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || 
            !System.Net.IPAddress.TryParse(ipAddress, out _))
        {
            return AuditErrors.InvalidIpAddress;
        }
        
        return new IpAddress(ipAddress);
    }
    
    public static Result<IpAddress> Create(System.Net.IPAddress? ipAddress)
    {
        if (ipAddress is null)
        {
            return AuditErrors.InvalidIpAddress;
        }
        
        return new IpAddress(ipAddress.ToString());
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}