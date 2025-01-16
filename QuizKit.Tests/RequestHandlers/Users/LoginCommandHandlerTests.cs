using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.Options;
using QuizKit.Core.RequestHandlers.Users;
using JwtFactory;

namespace QuizKit.Tests.RequestHandlers.Users;

public class LoginCommandHandlerTests
{
    private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly JwtProvider _jwtProvider;
    private readonly UserPolicyOptions _userPolicyOptions;
    private readonly QuizDbContext _dbContext;

    public LoginCommandHandlerTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher<string>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _jwtProvider = new JwtProvider(new JwtInfo
        {
            Issuer = "test",
            Audience = "test",
            SecretKey = "testtesttesttesttesttesttesttesttesttesttest", // Must be at least 32 characters
        });
        _userPolicyOptions = new UserPolicyOptions
        {
            EnableLockout = true,
            MaxPasswordFailCount = 3,
            PasswordLockoutDuration = 30
        };

        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new QuizDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_LogsInUser()
    {
        // Arrange
        var user = new UserProfile(new SignUpCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        });
        user.SetPassword("hashedPassword");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(command.Email!, "hashedPassword", command.Password!))
            .Returns(PasswordVerificationResult.Success);

        var handler = new LoginCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _mockHttpContextAccessor.Object,
            _jwtProvider,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(updatedUser);
        Assert.NotNull(updatedUser.LastLogin);
        Assert.Equal(0, updatedUser.AccessFailedCount);
        Assert.Null(updatedUser.LockoutExpiry);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsError()
    {
        // Arrange
        var user = new UserProfile(new SignUpCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        });
        user.SetPassword("hashedPassword");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(command.Email!, "hashedPassword", command.Password!))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new LoginCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _mockHttpContextAccessor.Object,
            _jwtProvider,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal("Invalid email or password", result.Message);

        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(updatedUser);
        Assert.Equal(1, updatedUser.AccessFailedCount);
    }

    [Fact]
    public async Task Handle_WithNonexistentEmail_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        var handler = new LoginCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _mockHttpContextAccessor.Object,
            _jwtProvider,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal("Invalid email or password", result.Message);
    }

    [Fact]
    public async Task Handle_WithLockedAccount_ReturnsError()
    {
        // Arrange
        var user = new UserProfile(new SignUpCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        });
        user.SetPassword("hashedPassword");
        user.LogAccessFailure(true, 3, 30);
        user.LogAccessFailure(true, 3, 30);
        user.LogAccessFailure(true, 3, 30);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var handler = new LoginCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _mockHttpContextAccessor.Object,
            _jwtProvider,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal("Account is locked. Please try again later.", result.Message);
    }

    [Fact]
    public async Task Handle_WithMaxFailedAttempts_LocksAccount()
    {
        // Arrange
        var user = new UserProfile(new SignUpCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        });
        user.SetPassword("hashedPassword");
        user.LogAccessFailure(true, 3, 30); // First attempt
        user.LogAccessFailure(true, 3, 30); // Second attempt
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(command.Email!, "hashedPassword", command.Password!))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new LoginCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _mockHttpContextAccessor.Object,
            _jwtProvider,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal("Invalid email or password", result.Message);

        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(updatedUser);
        Assert.Equal(3, updatedUser.AccessFailedCount);
        Assert.NotNull(updatedUser.LockoutExpiry);
        Assert.True(updatedUser.IsAccountLocked);
    }
}
