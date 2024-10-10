using QuizKit.Common.Requests.Users;
using QuizKit.Core.Utils;

namespace QuizKit.Core.Entities;

public record UserProfile : IdEntity
{

    public UserProfile(SignUpCommand model)
        : base()
    {
        FirstName = model.FirstName?.Trim().ToLower();
        LastName = model.LastName?.Trim().ToLower();
        if (string.IsNullOrEmpty(model.Email))
            throw new ArgumentException("Email cannot be empty");
        Email = model.Email.Trim().ToLower();
        PhoneNumber = model.PhoneNumber;
        DateCreated = DateTime.UtcNow;
    }

    public string? FirstName { get; protected set; }
    public string? LastName { get; protected set; }
    public string? PasswordHash { get; protected set; }
    public string? Email { get; protected set; }
    public string? PhoneNumber { get; protected set; }
    public int AccessFailedCount { get; protected set; }
    public DateTime? LockoutExpiry { get; protected set; }
    public DateTime DateCreated { get; protected set; }
    public DateTime? LastPasswordChange { get; protected set; }
    public string? PasswordToken { get; protected set; }
    public DateTime? PasswordTokenExpiry { get; protected set; }
    public string? CreatorId { get; set; }
    public DateTime? LastLogin { get; protected set; }
    public bool IsAccountLocked => LockoutExpiry.HasValue && LockoutExpiry.Value > DateTime.UtcNow;

    public string Name => $"{FirstName} {LastName}";

    protected readonly List<UserRole> _roles = [];
    protected readonly List<UserOrganization> _organizations = [];

    public IReadOnlyList<UserRole> Roles => _roles;
    public IReadOnlyList<UserOrganization> Organizations => _organizations;

    public bool IsPasswordTokenExpired =>
        !string.IsNullOrEmpty(PasswordToken) &&
        PasswordTokenExpiry.HasValue &&
        DateTime.UtcNow > PasswordTokenExpiry;

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        PasswordToken = null;
        PasswordTokenExpiry = null;
        LastPasswordChange = DateTime.UtcNow;

        // Unlock account
        LockoutExpiry = null;
    }

    public void SetPasswordToken(string token, int expiryMinutes)
    {
        PasswordToken = token;
        PasswordTokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    public UserRole AddRole(string roleName)
    {
        var role = new UserRole(Id, roleName);
        if (_roles.Any(r => r.Role.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"User already in role '{roleName}'");
        _roles.Add(role);
        return role;
    }

    public void LogAccessFailure(bool lockoutEnabled, int maxFailCount, int lockoutMinutes)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxFailCount)
        {
            LockoutExpiry = lockoutEnabled ? DateTime.UtcNow.AddMinutes(lockoutMinutes) : null;
        }
        // TODO: Log access failure to UserAudit table
    }

    public void LogAccessSuccess()
    {
        LastLogin = DateTime.UtcNow;
        AccessFailedCount = 0;
        LockoutExpiry = null;
        // TODO: Log access success to UserAudit table
    }
}

