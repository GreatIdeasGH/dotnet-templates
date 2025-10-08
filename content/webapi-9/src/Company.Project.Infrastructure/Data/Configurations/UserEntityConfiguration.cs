namespace Company.Project.Infrastructure.Data.Configurations;

internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.HasIndex(p => p.PhoneNumber).IsUnique();

        // IP Address tracking
        //builder
        //    .Property(e => e.LastLoginIpAddress)
        //    .HasConversion(
        //        v => v != null ? v.ToString() : null,
        //        v => v != null ? System.Net.IPAddress.Parse(v) : null
        //    )
        //    .HasMaxLength(45); // IPv6 max length

        //builder.Property(e => e.LastLoginLocation).HasMaxLength(200);

        // Indexes for tracking
        //builder.HasIndex(e => e.LastLoginAt).HasDatabaseName("IX_Users_LastLoginAt");

        //builder.HasIndex(e => e.LastLoginIpAddress).HasDatabaseName("IX_Users_LastLoginIpAddress");
    }
}
