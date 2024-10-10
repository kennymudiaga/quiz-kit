using QuizKit.Core.ServiceContracts;

namespace QuizKit.Core.Services;

public class TokenGenerator : ITokenGenerator
{
    public string GenerateToken()
    {
        //TODO: Implement a real token generator
        return "12345678";
    }
}
