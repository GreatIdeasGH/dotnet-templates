namespace GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;

public sealed class EntityPermission
{
    public EntityPermission()
    {
    }

    public EntityPermission(string name, string value, string groupName, string description)
    {
        Name = name;
        Value = value;
        GroupName = groupName;
        Description = description;
    }

    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? Description { get; set; }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(EntityPermission permission)
    {
        return permission.Value;
    }
}