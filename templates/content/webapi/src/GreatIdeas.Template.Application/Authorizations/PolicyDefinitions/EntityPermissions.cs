using GreatIdeas.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;

public static class EntityPermissions
{
    private static ReadOnlyCollection<EntityPermission?> GetAllPermissions()
    {
        var allPermissions = new List<EntityPermission?>();
        IEnumerable<object> permissionClasses = typeof(AppPermissions)
            .GetNestedTypes(BindingFlags.Static | BindingFlags.Public)
            .Cast<TypeInfo>();

        foreach (TypeInfo permissionClass in permissionClasses)
        {
            var permissions = permissionClass.DeclaredFields.Where(f => f.IsLiteral);
            IEnumerable<(EntityPermission applicationPermission, DisplayAttribute[] attributes)> enumerable()
            {
                foreach (var permission in permissions)
                {
                    var applicationPermission = new EntityPermission
                    {
                        Value = permission.GetValue(null)?.ToString()!,
                        Name = permission.GetValue(null)?.ToString()?.Replace('.', ' ')
                                    .InsertSpaceBeforeUpperCase()!,
                        GroupName = permissionClass.Name
                    };
                    var attributes = (DisplayAttribute[])permission
                                                                    .GetCustomAttributes(typeof(DisplayAttribute), false);
                    yield return (applicationPermission, attributes);
                }
            }

            foreach (var (applicationPermission, attributes) in enumerable())
            {
                applicationPermission.Description = attributes.Length > 0
                                                                    ? attributes[0].Description!
                                                                    : applicationPermission.Name;
                allPermissions.Add(applicationPermission);
            }
        }

        return allPermissions.AsReadOnly();
    }

    public static EntityPermission? GetPermissionByName(string permissionName)
    {
        return GetAllPermissions().FirstOrDefault(p => p?.Name == permissionName);
    }

    public static EntityPermission? GetPermissionByValue(string permissionValue)
    {
        return GetAllPermissions().FirstOrDefault(p => p?.Value == permissionValue);
    }

    public static string?[] GetAllPermissionValues()
    {
        return GetAllPermissions().Select(p => p?.Value).ToArray();
    }

    public static string?[] GetAllPermissionNames()
    {
        return GetAllPermissions().Select(p => p?.Name).ToArray();
    }
}