using GreatIdeas.Template.Application.ServiceBuilders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GreatIdeas.Template.Infrastructure.ServiceBuilders;

internal static class IdentityJwtServiceCollection
{
    public static WebApplicationBuilder AddJwtAuthService(
        this WebApplicationBuilder builder,
        ApplicationSettings applicationSettings
    )
    {
        // Identity
        builder
            .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddRoles<IdentityRole>();

        var secret = $"{applicationSettings?.JwtSettings.Secret}";
        var validAudience = $"{applicationSettings?.JwtSettings.ValidAudience}";
        var validIssuer = $"{applicationSettings?.JwtSettings.ValidIssuer}";

        // Adding Authentication
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = validAudience,
            ValidIssuer = validIssuer,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            NameClaimType = JwtClaimTypes.GivenName,
            RoleClaimType = JwtClaimTypes.Role,
        };

        builder.Services.AddSingleton(tokenValidationParameters);

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
            });

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        builder.Services.AddAuthorization();
        builder.Services.AddAuthorizationPolicies();
        builder.Services.AddScoped<JwtService>();

        return builder;
    }
}
