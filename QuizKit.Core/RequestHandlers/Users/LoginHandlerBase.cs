using AutoMapper;
using JwtFactory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using QuizKit.Core.Entities;
using QuizKit.Core.Options;
using System.Security.Claims;
using QuizKit.Common.Models.Users;

namespace QuizKit.Core.RequestHandlers.Users;

public abstract class LoginHandlerBase(
IHttpContextAccessor httpContextAccessor,
JwtProvider jwtProvider,
UserPolicyOptions userPolicy)
{
    protected IHttpContextAccessor HttpContextAccessor = httpContextAccessor;
    protected readonly JwtProvider JwtProvider = jwtProvider;
    protected readonly UserPolicyOptions UserPolicy = userPolicy;

    protected async Task<LoggedInUserModel> CreateLogin(UserProfile userProfile)
    {
        var tokenExpiryDate = DateTime.UtcNow.AddMinutes(UserPolicy.SessionTimeout);
        var claims = GetClaims(userProfile);
        string? jwtToken = null;

        if (UserPolicy.AuthenticationScheme == JwtBearerDefaults.AuthenticationScheme)
        {
            jwtToken = JwtProvider.GetUserToken(claims, tokenExpiryDate);
        }
        else if (UserPolicy.AuthenticationScheme == CookieAuthenticationDefaults.AuthenticationScheme)
        {
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            if (HttpContextAccessor.HttpContext != null)
                await HttpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = tokenExpiryDate,
                        AllowRefresh = true,
                        IsPersistent = true,
                        IssuedUtc = DateTime.UtcNow,
                    });
        }

        return new()
        {
            Id = userProfile.Id,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            Email = userProfile.Email,
            PhoneNumber = userProfile.PhoneNumber,
            Token = jwtToken,
            TokenExpiration = tokenExpiryDate,
            Organizations = userProfile.Organizations.Select(o => new UserOrganizationModel
            {
                OrganizationId = o.OrganizationId,
                Role = o.Role
            }).ToList(),
            Roles = userProfile.Roles.Select(r => r.Role).ToList()
        };
    }

    private static List<Claim> GetClaims(UserProfile userProfile)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userProfile.Name),
            new(ClaimTypes.Email, userProfile.Email!),
            new(ClaimTypes.Sid, userProfile.Id),
            new(ClaimTypes.GivenName, userProfile.FirstName ?? ""),
            new(ClaimTypes.Surname, userProfile.LastName ?? ""),
        };

        var roleClaims = userProfile.Roles.Select(r => new Claim(ClaimTypes.Role, r.Role));
        claims.AddRange(roleClaims);

        return claims;
    }
}
