using Company.Project.Application.Features.Audits;
using Company.Project.Infrastructure.Data;

namespace Company.Project.Infrastructure.Repositories;

internal sealed class AuditRepository(ApplicationDbContext context, ILogger<AuditRepository> logger)
    : IAuditRepository
{
    private static readonly ActivitySource ActivitySource = new(nameof(AuditRepository));

    public async ValueTask<AuditDetailResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(GetByIdAsync),
            ActivityKind.Server
        );
        activity?.Start();
        // Get Staff by id
        var result = await context
            .AuditTrails.AsNoTracking()
            .Where(x => x.AuditTraiId == id)
            .Select(x => new AuditDetailResponse
            {
                Id = x.AuditTraiId,
                Username = x.Username,
                FullName = x.FullName,
                Action = x.Action,
                TableName = x.TableName,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                AffectedColumns = x.AffectedColumns,
                Message = x.Message,
                Timestamp = x.Timestamp,
                IpAddress = x.IpAddress!.ToString(),
            })
            .FirstOrDefaultAsync(cancellationToken);
        return result;
    }

    public async ValueTask<IPagedList<AuditResponse>> GetPagedAuditsAsync(
        AuditPagingParameters pagingParameters,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(GetPagedAuditsAsync),
            ActivityKind.Server
        );
        activity?.Start();

        // Get paged audits
        IQueryable<AuditTrail>? query = FilterAudits(pagingParameters);

        var result = await query
            .Select(x => new AuditResponse
            {
                Id = x.AuditTraiId,
                Username = x.Username,
                Message = x.Message,
                Timestamp = x.Timestamp,
                Action = x.Action,
                IpAddress = x.IpAddress!.ToString(),
            })
            .ToPagedListAsync(
                pagingParameters.PageNumber,
                pagingParameters.PageSize,
                null,
                cancellationToken
            );

        logger.Retrieve(result.Count, nameof(AuditTrail));

        return result;
    }

    private IQueryable<AuditTrail> FilterAudits(AuditPagingParameters pagingParameters)
    {
        var collections = context.AuditTrails.AsNoTracking().TagWith("FilterAuditLogs");

        // Filter
        if (!string.IsNullOrWhiteSpace(pagingParameters.Username))
        {
            var param = pagingParameters.Username.Trim();
            collections = collections.Where(a => EF.Functions.ILike(a.Username, param));
        }

        if (!string.IsNullOrWhiteSpace(pagingParameters.Action))
        {
            var param = pagingParameters.Action.Trim();
            collections = collections.Where(a => EF.Functions.ILike(a.Action, param));
        }

        if (!string.IsNullOrWhiteSpace(pagingParameters.FullName))
        {
            var param = $"%{pagingParameters.FullName!.Trim()}%";
            collections = collections.Where(a => EF.Functions.ILike(a.FullName, param));
        }

        if (pagingParameters.StartDate is not null && pagingParameters.EndDate is not null)
        {
            var startDate = DateTime.SpecifyKind(
                pagingParameters.StartDate.Value.Date,
                DateTimeKind.Utc
            );
            var endDate = DateTime.SpecifyKind(
                pagingParameters.EndDate.Value.Date.AddDays(1).AddTicks(-1),
                DateTimeKind.Utc
            );

            collections = collections.Where(m =>
                m.Timestamp >= startDate && m.Timestamp <= endDate
            );
        }

        // Search
        if (!string.IsNullOrWhiteSpace(pagingParameters.Search))
        {
            var searchPattern = $"%{pagingParameters.Search!.Trim()}%";
            collections = collections.Where(a =>
                EF.Functions.ILike(a.FullName, searchPattern)
                || EF.Functions.ILike(a.Action, searchPattern)
                || EF.Functions.ILike(a.AffectedColumns, searchPattern)
                || EF.Functions.ILike(a.TableName, searchPattern)
            // || EF.Functions.ILike(a.OldValues, searchPattern)
            // || EF.Functions.ILike(a.NewValues, searchPattern)
            );
        }

        // Sort
        collections = collections.OrderByDescending(a => a.Timestamp);

        return collections;
    }
}
