using GreatIdeas.Template.Application.Authorizations.Policies;
using GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;

namespace GreatIdeas.Template.Application.ServiceBuilders;

public static class AuthorizationServiceCollection
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();

        // Authorizations
        services.AddAuthorizationCore(options =>
        {
            // Add roles
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("User", policy => policy.RequireRole("User"));

            // Audit Policies
            options.AddPolicy(AppPermissions.Audit.View, AuditPolicy.CanView());
            options.AddPolicy(AppPermissions.Audit.Manage, AuditPolicy.CanManage());

            // Account Policies
            options.AddPolicy(AppPermissions.Account.View, AccountPolicy.CanView());
            options.AddPolicy(AppPermissions.Account.Manage, AccountPolicy.CanManage());
            options.AddPolicy(AppPermissions.Account.Delete, AccountPolicy.CanDelete());
        });

        return services;
    }
}
