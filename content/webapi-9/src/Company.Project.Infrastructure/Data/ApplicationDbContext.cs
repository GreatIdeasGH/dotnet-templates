namespace Company.Project.Infrastructure.Data;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ITenantService tenantService
) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<AuditTrail> AuditTrails { get; init; } = null!;
    public DbSet<UserSession> UserSessions { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PersistUpdates();
        await LogAuditWithEntityBase();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void PersistUpdates()
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = tenantService.Name;
                    entry.Entity.CreatedOn = tenantService.Timestamp;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedBy = tenantService.Name;
                    entry.Entity.DateModified = tenantService.Timestamp;
                    break;
            }
        }
    }

    private async Task LogAuditWithEntityBase()
    {
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            // Skip audit for certain entities
            if (
                entry.Entity is AuditTrail
                || entry.Entity is IdentityUserClaim<string>
                || entry.Entity is IdentityRoleClaim<string>
                || entry.Entity is IdentityUserRole<string>
                || entry.Entity is UserSession
                || entry.State == EntityState.Detached
                || entry.State == EntityState.Unchanged
            )
            {
                continue;
            }

            // Create new audit entry
            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Entity.GetType().Name,
                Username = tenantService.Username!,
                FullName = tenantService.Name!,
                IpAddress = await tenantService.GetIpAddress(),
            };

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                var originalValue = property.OriginalValue;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues![propertyName] = property.CurrentValue!;
                    continue;
                }

                if (!SensitiveProperties.Contains(property.Metadata.Name))
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            break;

                        case EntityState.Deleted:
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues![propertyName] = originalValue!;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues![propertyName] = originalValue!;
                                auditEntry.NewValues![propertyName] = property.CurrentValue!;
                            }
                            break;
                    }
                }
            }
        }

        foreach (
            var auditEntry in auditEntries.Where(auditEntry =>
                !string.IsNullOrWhiteSpace(auditEntry.Username)
            )
        )
        {
            AuditTrails.Add(auditEntry.ToAudit());
        }
    }

    public static async Task<int> MarkAsDeleted<T>(
        IQueryable<T> query,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken
    )
        where T : class
    {
        var entityEntry = dbContext.Entry<T>((await query.FirstOrDefaultAsync(cancellationToken))!);
        entityEntry.State = EntityState.Deleted;
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static readonly List<string> SensitiveProperties =
    [
        "PasswordHash",
        "RefreshToken",
        "SecurityStamp",
    ];
}
