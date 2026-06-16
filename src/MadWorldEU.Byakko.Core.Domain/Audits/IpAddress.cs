namespace MadWorldEU.Byakko.Audits;

/// <summary>Value object representing a validated IP address (IPv4 or IPv6).</summary>
public sealed class IpAddress : ValueObject
{
    /// <summary>The IP address as a string.</summary>
    public string Value { get; }

    private IpAddress(string ipAddress)
    {
        Value = ipAddress;
    }

    /// <summary>Creates an <see cref="IpAddress"/> from a string, validating it is a valid IP address.</summary>
    public static Result<IpAddress> Create(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || 
            !System.Net.IPAddress.TryParse(ipAddress, out _))
        {
            return AuditErrors.InvalidIpAddress;
        }
        
        return new IpAddress(ipAddress);
    }
    
    /// <summary>Creates an <see cref="IpAddress"/> from a <see cref="System.Net.IPAddress"/>, returning a failure if null.</summary>
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