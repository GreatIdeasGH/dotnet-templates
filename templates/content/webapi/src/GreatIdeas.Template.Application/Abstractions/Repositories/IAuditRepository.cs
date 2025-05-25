using GreatIdeas.Template.Application.Features.Audits;

namespace GreatIdeas.Template.Application.Abstractions.Repositories;

public interface IAuditRepository
{
    ValueTask<AuditDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    ValueTask<IPagedList<AuditResponse>> GetPagedAuditsAsync(
        AuditPagingParameters pagingParameters,
        CancellationToken cancellationToken
    );
}
