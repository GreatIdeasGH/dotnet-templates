using GreatIdeas.Template.Application.Authorizations.Policies;
using GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;

namespace GreatIdeas.Template.Application.ServiceBuilders;

public static class AuthorizationServiceCollection
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();

        services.AddAuthorizationCore(options =>
        {
            // Audit Policies
            options.AddPolicy(AppPermissions.Audit.View, AuditPolicy.CanViewAudit());
            options.AddPolicy(AppPermissions.Audit.Manage, AuditPolicy.CanManageAudit());
        });

        return services;
    }
}
