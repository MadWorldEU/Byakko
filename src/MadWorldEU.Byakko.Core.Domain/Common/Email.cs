using System.Globalization;
using System.Text.RegularExpressions;

namespace MadWorldEU.Byakko.Common;

/// <summary>Value object representing a validated email address.</summary>
public sealed class Email : ValueObject
{
    /// <summary>The normalized email address string.</summary>
    public string Value { get; }

    private Email(string value)
    {
        Value = value;   
    }
    
    /// <summary>Creates an <see cref="Email"/> from the given string, returning a failure if empty or invalid.</summary>
    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return EmailErrors.Empty;
        }

        if (!IsValidEmail(email))
        {
            return EmailErrors.Invalid;
        }

        return new Email(email);
    }
    
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}