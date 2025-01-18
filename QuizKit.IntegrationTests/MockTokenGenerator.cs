using QuizKit.Common.Enums;
using QuizKit.Core.ServiceContracts;

namespace QuizKit.IntegrationTests;

public class MockTokenGenerator : ITokenGenerator
{
    public string GenerateToken(int length = 6, TokenType tokenType = TokenType.Numeric)
    {
        return "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ"[..length];
    }
}
