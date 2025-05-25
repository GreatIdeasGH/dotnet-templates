using System.Reflection;
using GreatIdeas.Template.Application.Common.Constants;
using Microsoft.OpenApi.Models;

namespace GreatIdeas.Template.WebAPI.OpenApi;

public static class SwaggerOpenApiServices
{
    public static IServiceCollection AddSwaggerOpenApiServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Version = OtelConstants.ServiceVersion,
                    Title = OpenApiSwaggerConstants.ApiTitle,
                    Description = OpenApiSwaggerConstants.ApiDescription,
                    TermsOfService = new Uri($"{EmailDetails.Website}/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Administrator",
                        Email = "admin@email.com",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License",
                        Url = new Uri($"{EmailDetails.Website}/license"),
                    },
                }
            );

            //To Enable authorization using Swagger (JWT)
            swagger.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                }
            );

            swagger.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            swagger.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    public static void UseSwaggerOpenApiServices(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "docs";
            options.EnableFilter();
            options.DocumentTitle = OpenApiSwaggerConstants.ApiTitle;
            options.SwaggerEndpoint("/swagger/v1/swagger.json", OpenApiSwaggerConstants.ApiTitle);
        });

        // ReDoc
        app.UseReDoc(options =>
        {
            options.RoutePrefix = "redocs";
            options.DocumentTitle = OpenApiSwaggerConstants.ApiTitle;
            options.SpecUrl = "/swagger/v1/swagger.json";
        });
    }
}
