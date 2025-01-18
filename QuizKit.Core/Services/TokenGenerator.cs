using QuizKit.Common.Enums;
using QuizKit.Core.ServiceContracts;
using System.Security.Cryptography;

namespace QuizKit.Core.Services;

public class TokenGenerator : ITokenGenerator
{
    private const int MaxTokenLength = 50;
    private static readonly char[] Alphabetic = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'];
    private static readonly char[] Numeric = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    private static readonly char[] Alphanumeric = [.. Numeric, .. Alphabetic];
    private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    public string GenerateToken(int length = 6, TokenType tokenType = TokenType.Numeric)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be greater than 0", nameof(length));
        if (length > MaxTokenLength)
            throw new ArgumentException($"Length must not exceed {MaxTokenLength}", nameof(length));

        var charset = tokenType switch
        {
            TokenType.Numeric => Numeric,
            TokenType.Alphabetic => Alphabetic,
            TokenType.Alphanumeric => Alphanumeric,
            _ => throw new ArgumentException("Unknown token type.", nameof(tokenType))
        };

        var result = new char[length];
        var buffer = new byte[4];

        for (var i = 0; i < length; i++)
        {
            rng.GetBytes(buffer);
            var randomValue = BitConverter.ToUInt32(buffer, 0);
            result[i] = charset[randomValue % charset.Length];
        }

        return new string(result);
    }
}
