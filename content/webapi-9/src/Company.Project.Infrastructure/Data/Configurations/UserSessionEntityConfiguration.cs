namespace Company.Project.Infrastructure.Data.Configurations;

public sealed class UserSessionEntityConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id).IsRequired().ValueGeneratedNever();

        builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);

        builder
            .Property(e => e.IpAddress)
            .HasConversion(
                v => v != null ? v.ToString() : null,
                v => v != null ? System.Net.IPAddress.Parse(v) : null
            )
            .HasMaxLength(45); // IPv6 max length

        builder.Property(e => e.UserAgent).HasMaxLength(1000);

        builder.Property(e => e.DeviceInfo).HasMaxLength(100);

        builder.Property(e => e.Location).HasMaxLength(200);

        builder.Property(e => e.Country).HasMaxLength(100);

        builder.Property(e => e.City).HasMaxLength(100);

        builder.Property(e => e.Region).HasMaxLength(100);

        builder.Property(e => e.SessionToken).HasMaxLength(50);

        builder.Property(e => e.LoginAt).IsRequired();

        builder.Property(e => e.LastActivityAt).IsRequired();

        builder.Property(e => e.LogoutAt).IsRequired(false);

        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        // Relationships
        builder
            .HasOne(e => e.User)
            .WithMany(u => u.UserSessions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // builder.HasIndex(e => e.UserId).HasDatabaseName("IX_UserSessions_UserId");

        // builder
        //     .HasIndex(e => e.SessionToken)
        //     .IsUnique()
        //     .HasDatabaseName("IX_UserSessions_SessionToken")
        //     .HasFilter("[SessionToken] IS NOT NULL");

        // builder.HasIndex(e => e.IpAddress).HasDatabaseName("IX_UserSessions_IpAddress");

        // builder.HasIndex(e => e.LoginAt).HasDatabaseName("IX_UserSessions_LoginAt");

        // builder.HasIndex(e => e.IsActive).HasDatabaseName("IX_UserSessions_IsActive");

        // // Composite indexes
        // builder
        //     .HasIndex(e => new { e.UserId, e.IsActive })
        //     .HasDatabaseName("IX_UserSessions_UserId_IsActive");

        // builder
        //     .HasIndex(e => new { e.UserId, e.LoginAt })
        //     .HasDatabaseName("IX_UserSessions_UserId_LoginAt");
    }
}
