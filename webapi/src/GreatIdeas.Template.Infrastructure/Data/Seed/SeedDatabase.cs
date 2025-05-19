using GreatIdeas.Template.Application.Authorizations.Identity;
using GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;
using GreatIdeas.Template.Application.Common.Constants;
using GreatIdeas.Template.Application.Common.Exceptions;
using GreatIdeas.Template.Domain.Entities;
using GreatIdeas.Template.Domain.Enums;
using Microsoft.Extensions.Hosting;

namespace GreatIdeas.Template.Infrastructure.Data.Seed;

public static class SeedDatabase
{
    private static readonly ActivitySource ActivitySource = new(nameof(SeedDatabase));

    public static async Task MigrateDb(IApplicationBuilder builder, IHostEnvironment environment)
    {
        // Start activity
        using var activity = ActivitySource.StartActivity(nameof(MigrateDb), ActivityKind.Consumer);

        // seed the database.  Best practice = in Main, using service scope
        using var scope = builder.ApplicationServices.CreateScope();

        try
        {
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<
                UserManager<ApplicationUser>
            >();

            #region Setup Permissions and Roles

            // System Admin
            var getSysAdmin = roleManager.FindByNameAsync(UserRoles.Admin).Result;
            if (getSysAdmin == null)
            {
                var sysAdminRole = new IdentityRole { Name = UserRoles.Admin };

                var result = roleManager.CreateAsync(sysAdminRole).Result;
                if (result.Succeeded)
                {
                    // Add all Permissions
                    var allClaims = EntityPermissions
                        .GetAllPermissionValues()
                        .Distinct()
                        .Except([AppPermissions.Score.Manage])
                        .ToArray();
                    foreach (var claim in allClaims)
                    {
                        _ = roleManager
                            .AddClaimAsync(sysAdminRole, new Claim("permission", claim!))
                            .Result;
                    }

                    Log.Information("Created {Role}", sysAdminRole);
                }
                else
                {
                    var message = result.Errors.First().Description;
                    OtelConstants.AddExceptionEvent(
                        activity,
                        new MigrationException(message),
                        message
                    );
                }
            }

            // Admin Role
            var getAdminRole = roleManager.FindByNameAsync(UserRoles.Admin).Result;
            if (getAdminRole == null)
            {
                var adminRole = new IdentityRole { Name = UserRoles.Admin, };

                var result = roleManager.CreateAsync(adminRole).Result;
                if (result.Succeeded)
                { // Seed admin permissions
                    List<Claim> claims =
                    [
                        new Claim(UserClaims.Permission, AppPermissions.Audit.View),
                    ];
                    foreach (var claim in claims)
                    {
                        _ = roleManager.AddClaimAsync(adminRole, claim).Result;
                    }

                    Log.Information("Created {Role}", adminRole);
                }
                else
                {
                    var message = result.Errors.First().Description;
                    OtelConstants.AddExceptionEvent(
                        activity,
                        new MigrationException(message),
                        message
                    );
                }
            }

            // User Role
            var getUserRole = roleManager.FindByNameAsync(UserRoles.User).Result;
            if (getUserRole == null)
            {
                var userRole = new IdentityRole { Name = UserRoles.User, };

                var result = roleManager.CreateAsync(userRole).Result;
                if (result.Succeeded)
                {
                    // Seed user permissions
                    List<Claim> claims =
                    [
                        new Claim(UserClaims.Permission, AppPermissions.Audit.View),
                    ];

                    foreach (var claim in claims)
                    {
                        _ = roleManager.AddClaimAsync(userRole, claim).Result;
                    }
                    Log.Information("Created {Role}", userRole);
                }
                else
                {
                    var message = result.Errors.First().Description;
                    OtelConstants.AddExceptionEvent(
                        activity,
                        new MigrationException(message),
                        message
                    );
                }
            }

            #endregion

            // Seed Data
            await SeedUsers(context, userManager);
        }
        catch (Exception ex)
        {
            OtelConstants.AddExceptionEvent(
                activity,
                ex,
                "An error occurred while migrating or seeding the database."
            );
        }
    }

    private static async Task SeedUsers(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager
    )
    {
        // Start activity
        using var activity = ActivitySource.StartActivity(nameof(SeedUsers), ActivityKind.Consumer);

            // Create Admin User    
            if (!await context.Users.AnyAsync(x => x.UserName == "admin"))
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@email.com",
                    EmailConfirmed = true,
                    PhoneNumber = "0123456789",
                    PhoneNumberConfirmed = true,
                    AccountType = AccountType.Admin.Name,
                    IsActive = true
                };
                await userManager.CreateAsync(adminUser, "P@ssword1");
                await userManager.AddClaimsAsync(
                    adminUser,
                    [
                        new Claim(JwtClaimTypes.Id, $"{adminUser.Id}"),
                        new Claim(UserClaims.Username, adminUser.UserName!),
                        new Claim(JwtClaimTypes.Name, "Admin Name"),
                        new Claim(UserClaims.AccountType, adminUser.AccountType),
                        new Claim(JwtClaimTypes.Email, adminUser.Email),
                    ]
                );
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
            }
            
            // Add User
            if (!await context.Users.AnyAsync(x => x.UserName == "user"))
            {
                var user = new ApplicationUser
                {
                    UserName = "user",
                    Email = "user@email.com",
                    EmailConfirmed = true,
                    PhoneNumber = "0123456780",
                    PhoneNumberConfirmed = true,
                    AccountType = AccountType.Admin.Name,
                    IsActive = true
                };
                await userManager.CreateAsync(user, "P@ssword1");
                await userManager.AddClaimsAsync(
                    user,
                    [
                        new Claim(JwtClaimTypes.Id, $"{user.Id}"),
                        new Claim(UserClaims.Username, user.UserName!),
                        new Claim(JwtClaimTypes.Name, "User Name"),
                        new Claim(UserClaims.AccountType, user.AccountType),
                        new Claim(JwtClaimTypes.Email, user.Email),
                    ]
                );
                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

        
    }
}
