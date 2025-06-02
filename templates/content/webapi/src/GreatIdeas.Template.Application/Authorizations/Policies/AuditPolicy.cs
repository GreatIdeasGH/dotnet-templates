using GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;

namespace GreatIdeas.Template.Application.Authorizations.Policies;

public static class AuditPolicy
{
    #region Audit Policies

    public static AuthorizationPolicy CanView()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Audit.View))
            .Build();
    }

    public static AuthorizationPolicy CanManage()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Audit.Manage))
            .Build();
    }

    #endregion
}
