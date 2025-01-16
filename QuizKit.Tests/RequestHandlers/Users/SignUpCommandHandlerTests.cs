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

public class SignUpCommandHandlerTests
{
    private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly JwtProvider _jwtProvider;
    private readonly UserPolicyOptions _userPolicyOptions;
    private readonly QuizDbContext _dbContext;

    public SignUpCommandHandlerTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher<string>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _jwtProvider = new JwtProvider(new JwtInfo
        {
            Issuer = "test",
            Audience = "test",
            SecretKey = "testtesttesttesttesttesttesttesttesttesttest", // Must be at least 32 characters
        });
        _userPolicyOptions = new UserPolicyOptions();

        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new QuizDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesUser()
    {
        // Arrange
        var command = new SignUpCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        _mockPasswordHasher
            .Setup(x => x.HashPassword(command.Email, command.Password))
            .Returns("hashedPassword");

        var handler = new SignUpCommandHandler(
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
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(user);
        Assert.Equal(command.FirstName?.ToLower(), user.FirstName);
        Assert.Equal(command.LastName?.ToLower(), user.LastName);
        Assert.Equal(command.Email.ToLower(), user.Email);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var existingUser = new UserProfile(new SignUpCommand
        {
            Email = "test@example.com",
            FirstName = "Existing",
            LastName = "User"
        });
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var command = new SignUpCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        var handler = new SignUpCommandHandler(
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
        Assert.Equal("Email is already registered", result.Message);
    }

    [Fact]
    public async Task Handle_WithValidInvitation_AddsUserToOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid().ToString();
        var inviteCode = "inviteCode123";
        var hashedInviteCode = "hashedInviteCode123";

        var invitation = new Invitation
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            TokenHash = hashedInviteCode,
            OrganizationId = organizationId,
            Role = "Member",
            Status = "Pending"
        };
        _dbContext.Invitations.Add(invitation);
        await _dbContext.SaveChangesAsync();

        var command = new SignUpCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            InviteCode = inviteCode
        };

        _mockPasswordHasher
            .Setup(x => x.HashPassword(command.Email, command.Password))
            .Returns("hashedPassword");

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(invitation.Email!, hashedInviteCode, inviteCode))
            .Returns(PasswordVerificationResult.Success);

        var handler = new SignUpCommandHandler(
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
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(user);
        var userOrg = await _dbContext.UserOrganizations.FirstOrDefaultAsync(x => x.UserProfileId == user.Id);
        Assert.NotNull(userOrg);
        Assert.Equal(organizationId, userOrg.OrganizationId);
        Assert.Equal("Member", userOrg.Role);

        var updatedInvitation = await _dbContext.Invitations.FindAsync(invitation.Id);
        Assert.NotNull(updatedInvitation);
        Assert.Equal("Accepted", updatedInvitation.Status);
    }

    [Fact]
    public async Task Handle_WithInvalidInviteCode_CreatesUserWithoutOrganization()
    {
        // Arrange
        var inviteCode = "invalidCode";
        var hashedInviteCode = "hashedInviteCode123";

        var invitation = new Invitation
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            TokenHash = hashedInviteCode,
            OrganizationId = Guid.NewGuid().ToString(),
            Role = "Member",
            Status = "Pending"
        };
        _dbContext.Invitations.Add(invitation);
        await _dbContext.SaveChangesAsync();

        var command = new SignUpCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            InviteCode = inviteCode
        };

        _mockPasswordHasher
            .Setup(x => x.HashPassword(command.Email, command.Password))
            .Returns("hashedPassword");

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(invitation.Email!, hashedInviteCode, inviteCode))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new SignUpCommandHandler(
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
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(user);
        var userOrg = await _dbContext.UserOrganizations.FirstOrDefaultAsync(x => x.UserProfileId == user.Id);
        Assert.Null(userOrg);
    }

    [Fact]
    public async Task Handle_WithExpiredInvitation_CreatesUserWithoutOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid().ToString();
        var invitation = new Invitation
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            TokenHash = "inviteCode123",
            OrganizationId = organizationId,
            Role = "Member",
            Status = "Expired"
        };
        _dbContext.Invitations.Add(invitation);
        await _dbContext.SaveChangesAsync();

        var command = new SignUpCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            InviteCode = "inviteCode123"
        };

        _mockPasswordHasher
            .Setup(x => x.HashPassword(command.Email, command.Password))
            .Returns("hashedPassword");

        var handler = new SignUpCommandHandler(
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
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(user);
        var userOrg = await _dbContext.UserOrganizations.FirstOrDefaultAsync(x => x.UserProfileId == user.Id);
        Assert.Null(userOrg);
    }
}
