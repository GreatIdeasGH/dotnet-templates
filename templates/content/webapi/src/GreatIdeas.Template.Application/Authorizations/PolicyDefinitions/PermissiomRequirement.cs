

namespace GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;

internal sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; private set; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

internal sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>,
        IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        if (
            context.User.Claims.Any(c =>
                c.Type == "permission" && c.Value == requirement.Permission
            )
        )
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
