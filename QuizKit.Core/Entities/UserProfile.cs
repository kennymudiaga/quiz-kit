using QuizKit.Common.Requests.Users;

namespace QuizKit.Core.Entities;

public record UserProfile : IdEntity
{
    public const string RolesNavigationName = nameof(_roles);
    public const string OrganizationsNavigationName = nameof(_organizations);

    private readonly List<UserRole> _roles = [];
    private readonly List<UserOrganization> _organizations = [];

    protected UserProfile()
    {
    }

    public UserProfile(SignUpCommand model)
        : this()
    {
        FirstName = model.FirstName?.Trim().ToLower();
        LastName = model.LastName?.Trim().ToLower();
        if (string.IsNullOrEmpty(model.Email))
            throw new ArgumentException("Email cannot be empty");
        Email = model.Email.Trim().ToLower();
        PhoneNumber = model.PhoneNumber;
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
    public string? PasswordTokenHash { get; protected set; }
    public DateTime? PasswordTokenExpiry { get; protected set; }
    public string? CreatorId { get; set; }
    public DateTime? LastLogin { get; protected set; }
    public bool IsAccountLocked => LockoutExpiry.HasValue && LockoutExpiry.Value > DateTime.UtcNow;

    public string Name => $"{FirstName} {LastName}";

    public IReadOnlyCollection<UserRole> Roles => _roles;
    public IReadOnlyCollection<UserOrganization> Organizations => _organizations;

    public bool IsPasswordTokenExpired =>
        !string.IsNullOrEmpty(PasswordTokenHash) &&
        PasswordTokenExpiry.HasValue &&
        DateTime.UtcNow > PasswordTokenExpiry;

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        PasswordTokenHash = null;
        PasswordTokenExpiry = null;
        LastPasswordChange = DateTime.UtcNow;

        // Unlock account
        LockoutExpiry = null;
    }

    public void SetPasswordToken(string token, int expiryMinutes)
    {
        PasswordTokenHash = token;
        PasswordTokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    public UserRole AddRole(string role)
    {
        var userRole = Roles.FirstOrDefault(x => x.Role == role);
        if (userRole is null)
        {
            userRole = new UserRole(Id, role);
            _roles.Add(userRole);
        }
        return userRole;
    }

    public void LogAccessFailure(bool lockoutEnabled, int maxFailCount, int lockoutMinutes)
    {
        AccessFailedCount++;
        if (lockoutEnabled && AccessFailedCount >= maxFailCount)
        {
            LockoutExpiry = DateTime.UtcNow.AddMinutes(lockoutMinutes);
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

    public void AddOrganization(string organizationId, string role)
    {
        if (_organizations.Any(x => x.OrganizationId == organizationId))
        {
            return;
        }

        _organizations.Add(new UserOrganization(Id, organizationId, role));
    }
}
