using QuizKit.Common.Enums;

namespace QuizKit.Core.ServiceContracts;

public interface ITokenGenerator
{
    string GenerateToken(int length = 6, TokenType tokenType = TokenType.Numeric);
}
