using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.Options;
using QuizKit.Core.RequestHandlers.Users;

namespace QuizKit.Tests.RequestHandlers.Users;

public class SendInvitationCommandHandlerTests
{
    private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher;
    private readonly UserPolicyOptions _userPolicyOptions;
    private readonly QuizDbContext _dbContext;

    public SendInvitationCommandHandlerTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher<string>>();
        _userPolicyOptions = new UserPolicyOptions
        {
            InviteExpiryHours = 24 // 1 hour
        };

        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new QuizDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesInvitation()
    {
        // Arrange
        var organizationId = Guid.NewGuid().ToString();
        var organization = new Core.Entities.Organization
        {
            Id = organizationId,
            Name = "Test Org"
        };
        _dbContext.Organizations.Add(organization);
        await _dbContext.SaveChangesAsync();

        var command = new SendInvitationCommand
        {
            Email = "test@example.com",
            OrganizationId = organizationId,
            Role = "Member"
        };

        _mockPasswordHasher
            .Setup(x => x.HashPassword(command.Email, It.IsAny<string>()))
            .Returns("hashedToken");

        var handler = new SendInvitationCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var invitation = await _dbContext.Invitations.FirstOrDefaultAsync(x => x.Email == command.Email);
        Assert.NotNull(invitation);
        Assert.Equal(organizationId, invitation.OrganizationId);
        Assert.Equal("Member", invitation.Role);
        Assert.Equal("Pending", invitation.Status);
        Assert.Equal("hashedToken", invitation.TokenHash);
        Assert.True(invitation.ExpiryDate > DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WithNonexistentOrganization_ReturnsError()
    {
        // Arrange
        var command = new SendInvitationCommand
        {
            Email = "test@example.com",
            OrganizationId = "nonexistent",
            Role = "Member"
        };

        var handler = new SendInvitationCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("Organization not found", result.Message);
    }

    [Fact]
    public async Task Handle_WithExistingMember_ReturnsError()
    {
        // Arrange
        var organizationId = Guid.NewGuid().ToString();
        var organization = new QuizKit.Core.Entities.Organization
        {
            Id = organizationId,
            Name = "Test Org"
        };
        _dbContext.Organizations.Add(organization);

        var user = new UserProfile(new SignUpCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        });
        user.AddOrganization(organizationId, "Member");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new SendInvitationCommand
        {
            Email = "test@example.com",
            OrganizationId = organizationId,
            Role = "Member"
        };

        var handler = new SendInvitationCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal("User is already a member of this organization", result.Message);
    }

    [Fact]
    public async Task Handle_WithExistingInvitation_ExtendsExpiry()
    {
        // Arrange
        var organizationId = Guid.NewGuid().ToString();
        var organization = new QuizKit.Core.Entities.Organization
        {
            Id = organizationId,
            Name = "Test Org"
        };
        _dbContext.Organizations.Add(organization);

        var existingInvitation = new Invitation
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            OrganizationId = organizationId,
            Role = "Member",
            TokenHash = "hashedToken",
            Status = "Pending",
            ExpiryDate = DateTime.UtcNow.AddMinutes(-30) // Expiry date 30 minutes ago
        };
        _dbContext.Invitations.Add(existingInvitation);
        await _dbContext.SaveChangesAsync();

        var command = new SendInvitationCommand
        {
            Email = "test@example.com",
            OrganizationId = organizationId,
            Role = "Member"
        };

        var handler = new SendInvitationCommandHandler(
            _dbContext,
            _mockPasswordHasher.Object,
            _userPolicyOptions
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var invitation = await _dbContext.Invitations.FirstOrDefaultAsync(x => x.Id == existingInvitation.Id);
        Assert.NotNull(invitation);
        Assert.Equal("Pending", invitation.Status);
        Assert.True(invitation.ExpiryDate > DateTime.UtcNow); // Expiry date should be extended
    }
}
