using System.IdentityModel.Tokens.Jwt;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Responses.Authentication;
using GreatIdeas.Template.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GreatIdeas.Template.Application.Common.Options;
using Microsoft.IdentityModel.Tokens;

namespace GreatIdeas.Template.Application.Services;

public sealed class JwtService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TimeProvider _dateTimeProvider;
    private readonly ILogger<JwtService> _logger;
    private readonly ApplicationSettings _applicationSettings;

    private string _secretKey = string.Empty;
    private string _validAudience = string.Empty;
    private string _validIssuer = string.Empty;
    private int _expiryTimeInHours;
    private int _refreshTokenExpiryInDays;

    public JwtService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TimeProvider dateTimeProvider,
        ILogger<JwtService> logger,
        IOptionsMonitor<ApplicationSettings> options
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _applicationSettings = options.CurrentValue;

        InitializeJwtOptions();
    }

    public RefreshTokenRequest GenerateRefreshToken()
    {
        var refreshToken = new RefreshTokenRequest();
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            refreshToken.Token = Convert.ToBase64String(randomNumber);
        }

        refreshToken.Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_refreshTokenExpiryInDays));

        return refreshToken;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        try
        {
            var tokenValidationParameters = TokenValidationParameters();

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                accessToken,
                tokenValidationParameters,
                out var securityToken
            );
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (
                jwtSecurityToken == null
                || !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                _logger.LogError("Invalid token");
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not process the request");
            throw new SecurityTokenException("Invalid token", e);
        }
    }

    public async Task<string> GenerateAccessToken(ApplicationUser user)
    {
        var claims = await GetClaims(user);
        var tokenDescriptor = SecurityTokenDescriptor(claims);
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(securityToken);
        return jwt;
    }

    public async Task<RefreshTokenResponse> ValidateRefreshToken(ApplicationUser user)
    {
        // Check if refresh token is expired
        var accessToken = await GenerateAccessToken(user);

        // If refresh token is expired
        var diff = _dateTimeProvider
            .GetUtcNow()
            .CompareTo(user.RefreshTokenExpiryTime ?? DateTimeOffset.MinValue);
        if (diff > 0)
        {
            _logger.LogWarning("Refresh token for user :{UserId} has expired", user.Id);

            // Generate new refresh token
            var generatedToken = GenerateRefreshToken();
            var newRefreshToken = new RefreshTokenResponse(
                AccessToken: accessToken,
                RefreshToken: generatedToken.Token,
                Expires: generatedToken.Expires
            );

            _logger.LogInformation("Refresh token for user: {UserId} has been updated", user.Id);
            return newRefreshToken;
        }

        // If refresh token is not expired
        _logger.LogWarning("Refresh token for user: {UserId} has not expired", user.Id);

        var exitingRefreshToken = new RefreshTokenResponse(
            AccessToken: accessToken,
            RefreshToken: user.RefreshToken,
            Expires: user.RefreshTokenExpiryTime
        );
        return exitingRefreshToken;
    }

    private void InitializeJwtOptions()
    {
        _secretKey = _applicationSettings.JwtSettings.Secret;
        _validAudience = _applicationSettings.JwtSettings.ValidAudience;
        _validIssuer = _applicationSettings.JwtSettings.ValidIssuer;
        _expiryTimeInHours = _applicationSettings.JwtSettings.ExpiryTimeInHours;
        _refreshTokenExpiryInDays = _applicationSettings.JwtSettings.RefreshTokenExpiryInDays;
    }

    private async Task<List<Claim>> GetClaims(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            // new Claim(JwtRegisteredClaimNames.Amr, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            var role = await _roleManager.FindByNameAsync(userRole);
            if (role == null)
                continue;
            var roleClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var roleClaim in roleClaims)
            {
                if (claims.Contains(roleClaim))
                    continue;

                claims.Add(roleClaim);
            }
        }
        return claims;
    }

    private TokenValidationParameters TokenValidationParameters()
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.UTF8.GetBytes(_secretKey)),
            ValidateLifetime = false,
            ValidIssuer = _validIssuer,
            ValidAudience = _validAudience,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(5),
        };
        return tokenValidationParameters;
    }

    private SecurityTokenDescriptor SecurityTokenDescriptor(List<Claim> claims)
    {
        var tokenDescriptors = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _validIssuer,
            Audience = _validAudience,
            Expires = _dateTimeProvider
                .GetLocalNow()
                .AddHours(Convert.ToDouble(_expiryTimeInHours))
                .DateTime,
            SigningCredentials = GetSigningCredentials(),
            NotBefore = _dateTimeProvider.GetLocalNow().DateTime,
        };
        return tokenDescriptors;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    public void SetTokenCookie(HttpContext httpContext, LoginResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            IsEssential = true,
            Secure = false,
            SameSite = SameSiteMode.Unspecified,
            Expires = _dateTimeProvider
                .GetLocalNow()
                .AddMinutes(Convert.ToDouble(_expiryTimeInHours)),
        };

        httpContext.Response.Cookies.Append("accessToken", response.AccessToken, cookieOptions);
    }
}
