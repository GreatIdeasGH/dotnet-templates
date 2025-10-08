using Company.Project.Application.Authorizations.PolicyDefinitions;

namespace Company.Project.Application.Authorizations.Policies;

public static class AppPolicy
{
    #region App Policies

    public static AuthorizationPolicy ViewDashboard()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Dashboard.View))
            .Build();
    }

    public static AuthorizationPolicy ViewSettings()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Settings.View))
            .Build();
    }

    public static AuthorizationPolicy ManageSettings()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(AppPermissions.Settings.Manage))
            .Build();
    }

    #endregion
}
