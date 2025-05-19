using GreatIdeas.Template.Domain.Entities;

namespace GreatIdeas.Template.Infrastructure.Data.Configurations;

internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.HasIndex(p => p.PhoneNumber).IsUnique();
    }
}
