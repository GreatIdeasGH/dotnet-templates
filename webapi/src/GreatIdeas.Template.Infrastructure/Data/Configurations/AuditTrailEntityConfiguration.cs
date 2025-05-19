namespace GreatIdeas.Template.Infrastructure.Data.Configurations;

internal sealed class AuditTrailEntityConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.HasKey(p => p.AuditTraiId);

        builder.Property(p => p.OldValues).HasColumnType("jsonb");
        builder.Property(p => p.NewValues).HasColumnType("jsonb");
    }
}
