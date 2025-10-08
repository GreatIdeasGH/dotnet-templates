using Microsoft.AspNetCore.Authentication.JwtBearer;

using Scalar.AspNetCore;

namespace Company.Project.WebAPI.Extensions;

internal static class ScalarOpenApiExtensions
{
    public static WebApplication MapOpenApiWithScalar(this WebApplication app)
    {
        app.MapOpenApi();

        // Scalar API reference
        app.MapScalarApiReference(
            "api/docs",
            options =>
            {
                options
                    .WithTitle("Fundraiser WebAPI")
                    .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
                    .AddHttpAuthentication(
                        JwtBearerDefaults.AuthenticationScheme,
                        opt =>
                        {
                            opt.Description = "Fundraiser WebAPI";
                        }
                    );
            }
        );
        return app;
    }
}
