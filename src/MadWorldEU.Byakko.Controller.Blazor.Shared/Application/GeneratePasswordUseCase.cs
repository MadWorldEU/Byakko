using System.Security.Cryptography;

namespace MadWorldEU.Byakko.Application;

/// <summary>
/// Generates a cryptographically secure random password containing at least one uppercase letter,
/// one lowercase letter, one digit, and one special character, shuffled via Fisher-Yates.
/// </summary>
public sealed class GeneratePasswordUseCase
{
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Special = "!@#$%^&*()-_=+[]{}<>?";
    private const int Length = 20;

    private static readonly string AllChars =
        Uppercase + Lowercase + Digits + Special;
    
    /// <summary>
    /// Generates and returns a 20-character random password.
    /// </summary>
    public string Execute()
    {
        var password = new char[Length];

        // Ensure at least one of each category
        password[0] = Uppercase[RandomNumberGenerator.GetInt32(Uppercase.Length)];
        password[1] = Lowercase[RandomNumberGenerator.GetInt32(Lowercase.Length)];
        password[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
        password[3] = Special[RandomNumberGenerator.GetInt32(Special.Length)];

        // Fill remaining characters
        for (int i = 4; i < Length; i++)
        {
            password[i] = AllChars[RandomNumberGenerator.GetInt32(AllChars.Length)];
        }

        // Fisher-Yates shuffle
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}