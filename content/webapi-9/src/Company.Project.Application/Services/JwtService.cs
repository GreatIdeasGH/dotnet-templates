using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Company.Project.Application.Common.Options;
using Company.Project.Application.Features.Account.Login;
using Company.Project.Application.Features.Account.RefreshToken;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Company.Project.Application.Services;

public sealed class JwtService
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly TimeProvider _dateTimeProvider;
    private readonly ILogger<JwtService> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private int _expiryTimeInMinutes;
    private int _refreshTokenExpiryInDays;

    private string _secretKey = string.Empty;
    private string _validAudience = string.Empty;
    private string _validIssuer = string.Empty;

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
                accessToken,
                generatedToken.RefreshToken,
                generatedToken.Expires
            );

            _logger.LogInformation("Refresh token for user: {UserId} has been updated", user.Id);
            return newRefreshToken;
        }

        // If refresh token is not expired
        _logger.LogWarning("Refresh token for user: {UserId} has not expired", user.Id);

        var exitingRefreshToken = new RefreshTokenResponse(
            accessToken,
            user.RefreshToken,
            user.RefreshTokenExpiryTime
        );
        return exitingRefreshToken;
    }

    private void InitializeJwtOptions()
    {
        _secretKey = _applicationSettings.JwtSettings.Secret;
        _validAudience = _applicationSettings.JwtSettings.ValidAudience;
        _validIssuer = _applicationSettings.JwtSettings.ValidIssuer;
        _expiryTimeInMinutes = _applicationSettings.JwtSettings.ExpiryTimeInMinutes;
        _refreshTokenExpiryInDays = _applicationSettings.JwtSettings.RefreshTokenExpiryInDays;
    }

    private RefreshTokenResponse GenerateRefreshToken()
    {
        var refreshToken = new RefreshTokenResponse();
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            refreshToken.RefreshToken = Convert.ToBase64String(randomNumber);
        }

        refreshToken.Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_refreshTokenExpiryInDays));

        return refreshToken;
    }

    private async Task<string> GenerateAccessToken(ApplicationUser user)
    {
        var claims = await GetClaims(user);
        var tokenDescriptor = SecurityTokenDescriptor(claims);
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(securityToken);
        return jwt;
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

    private TokenValidationParameters TokenValidationParameters()
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
            ValidateLifetime = false,
            ValidIssuer = _validIssuer,
            ValidAudience = _validAudience,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(5),
        };
        return tokenValidationParameters;
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
            {
                continue;
            }

            var roleClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var roleClaim in roleClaims)
            {
                if (claims.Contains(roleClaim))
                {
                    continue;
                }

                claims.Add(roleClaim);
            }
        }

        return claims;
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
                .AddMinutes(Convert.ToDouble(_expiryTimeInMinutes))
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
                .AddMinutes(Convert.ToDouble(_expiryTimeInMinutes)),
        };

        httpContext.Response.Cookies.Append("accessToken", response.AccessToken, cookieOptions);
    }
}
