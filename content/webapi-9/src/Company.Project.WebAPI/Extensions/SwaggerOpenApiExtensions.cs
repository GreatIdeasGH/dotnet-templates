using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

namespace Company.Project.WebAPI.Extensions;

internal static class SwaggerOpenApiExtensions
{
    public static WebApplicationBuilder AddSwaggerDocs(this WebApplicationBuilder builder)
    {
        // Register Swagger/OpenAPI services
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Company.Project.WebAPI",
                    Version = "v1",
                    Description = "Company.Project.WebAPI Documentation",
                }
            );
            options.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Description =
                        "Enter 'Bearer' [space] and then your token in the text input below.",
                }
            );
            options.AddSecurityRequirement(
                new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme,
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
        });

        return builder;
    }

    public static WebApplication MapOpenApiWithSwagger(this WebApplication app)
    {
        app.MapOpenApi();

        // Scalar API reference
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "/openapi/{documentName}.json";
        });
        app.MapScalarApiReference();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "api/docs";
            options.SwaggerEndpoint("/openapi/v1.json", "Company.Project.WebAPI");
            options.DocumentTitle = "Company.Project.WebAPI Documentation";
        });
        return app;
    }
}
