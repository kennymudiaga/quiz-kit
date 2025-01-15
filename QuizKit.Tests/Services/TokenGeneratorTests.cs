using QuizKit.Common.Enums;
using QuizKit.Core.Services;

namespace QuizKit.Tests.Services;

public class TokenGeneratorTests
{
    private readonly TokenGenerator _tokenGenerator;

    public TokenGeneratorTests()
    {
        _tokenGenerator = new TokenGenerator();
    }

    [Theory]
    [InlineData(TokenType.Numeric, 6)]
    [InlineData(TokenType.Alphabetic, 8)]
    [InlineData(TokenType.Alphanumeric, 10)]
    public void GenerateToken_ShouldReturnCorrectLength(TokenType tokenType, int length)
    {
        // Act
        var token = _tokenGenerator.GenerateToken(length, tokenType);

        // Assert
        Assert.Equal(length, token.Length);
    }

    [Theory]
    [InlineData(TokenType.Numeric)]
    public void GenerateToken_NumericType_ShouldOnlyContainNumbers(TokenType tokenType)
    {
        // Act
        var token = _tokenGenerator.GenerateToken(tokenType: tokenType);

        // Assert
        Assert.Matches("^[0-9]+$", token);
    }

    [Theory]
    [InlineData(TokenType.Alphabetic)]
    public void GenerateToken_AlphabeticType_ShouldOnlyContainLetters(TokenType tokenType)
    {
        // Act
        var token = _tokenGenerator.GenerateToken(tokenType: tokenType);

        // Assert
        Assert.Matches("^[a-z]+$", token);
    }

    [Theory]
    [InlineData(TokenType.Alphanumeric)]
    public void GenerateToken_AlphanumericType_ShouldContainLettersAndNumbers(TokenType tokenType)
    {
        // Act
        var token = _tokenGenerator.GenerateToken(tokenType: tokenType);

        // Assert
        Assert.Matches("^[a-z0-9]+$", token);
    }

    [Fact]
    public void GenerateToken_WithDefaultParameters_ShouldGenerateNumericTokenOfLength6()
    {
        // Act
        var token = _tokenGenerator.GenerateToken();

        // Assert
        Assert.Equal(6, token.Length);
        Assert.Matches("^[0-9]+$", token);
    }

    [Fact]
    public void GenerateToken_ShouldGenerateUniqueTokens()
    {
        // Arrange
        const int sampleSize = 100;
        var tokens = new HashSet<string>();

        // Act
        for (var i = 0; i < sampleSize; i++)
        {
            var token = _tokenGenerator.GenerateToken();
            tokens.Add(token);
        }

        // Assert
        Assert.Equal(sampleSize, tokens.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateToken_WithInvalidLength_ShouldThrowArgumentException(int length)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenGenerator.GenerateToken(length));
    }

    [Fact]
    public void GenerateToken_WithLengthExceedingMax_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenGenerator.GenerateToken(51));
    }
}
