using Company.Project.Application.Authorizations.PolicyDefinitions;

namespace Company.Project.Application.Authorizations.Policies;

public static class AuditPolicy
{
    #region Audit Policies

    public static AuthorizationPolicy CanViewAudit()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Audit.View))
            .Build();
    }

    public static AuthorizationPolicy CanManageAudit()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Audit.Manage))
            .Build();
    }

    #endregion
}
