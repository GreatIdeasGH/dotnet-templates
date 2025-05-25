using Microsoft.Extensions.Options;

namespace GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;

internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        // There can only be one policy provider in ASP.NET Core.
        // We only handle permissions related policies, for the rest
        // we will use the default provider.
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("Permissions", StringComparison.OrdinalIgnoreCase))
        {
            return await FallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        var policy = new AuthorizationPolicyBuilder();
        policy.AddRequirements(new PermissionRequirement(policyName));
        return await Task.FromResult(policy.Build());

        // Policy is not for permissions, try the default provider.
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return FallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public async Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return await FallbackPolicyProvider.GetDefaultPolicyAsync();
    }
}