using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace QuizKit.Core.Options;

public class UserPolicyOptions : OptionsBase
{
    public const string ConfigSection = "UserPolicy";

    /// <summary>
    /// Session timeout in minutes
    /// </summary>
    public virtual int SessionTimeout { get; init; } = 30;
    /// <summary>
    /// Gets or sets how long a password reset token is valid - in minutes
    /// </summary>
    public virtual int PasswordTokenTimeout { get; init; } = 5;

    /// <summary>
    /// Gets or sets how long an email verification token is valid - in minutes
    /// </summary>
    public virtual int EmailVerificationTimeout { get; init; } = 30;

    /// <summary>
    /// Determines if lockout is enabled for wrong password attempts
    /// </summary>
    public virtual bool EnableLockout { get; init; } = true;

    /// <summary>
    /// Gets or sets the number of wrong password attempts triggers a lockout - iff EnableLockout is <see langword="true"/>
    /// </summary>
    public virtual int MaxPasswordFailCount { get; init; } = 5;

    /// <summary>
    /// Gets or sets how long a password lockout will last - iff EnableLockout is <see langword="true"/>
    /// </summary>
    public virtual int PasswordLockoutDuration { get; init; } = 30;

    /// <summary>
    /// Gets or sets the timeout for an invite token - in hours
    /// </summary>
    public virtual int InviteExpiryHours { get; init; } = 72;

    public virtual string AuthenticationScheme { get; init; } = JwtBearerDefaults.AuthenticationScheme;
}
