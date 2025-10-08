using Company.Project.Application.Authorizations.PolicyDefinitions;

namespace Company.Project.Application.Authorizations.Policies;

public static class AccountPolicy
{
    #region Account Policies

    public static AuthorizationPolicy CanView()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Account.View))
            .Build();
    }

    public static AuthorizationPolicy CanManage()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Account.Manage))
            .Build();
    }

    public static AuthorizationPolicy CanDelete()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Account.Delete))
            .Build();
    }

    #endregion
}
