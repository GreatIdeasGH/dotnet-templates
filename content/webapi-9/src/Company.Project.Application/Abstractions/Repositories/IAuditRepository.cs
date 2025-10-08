using Company.Project.Application.Features.Audits;

namespace Company.Project.Application.Abstractions.Repositories;

public interface IAuditRepository
{
    ValueTask<AuditDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    ValueTask<IPagedList<AuditResponse>> GetPagedAuditsAsync(
        AuditPagingParameters pagingParameters,
        CancellationToken cancellationToken
    );
}
