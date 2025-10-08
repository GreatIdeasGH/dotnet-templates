namespace Company.Project.Application.Features.Audits;

public interface IGetPagedAuditHandler : IApplicationHandler
{
    ValueTask<ErrorOr<IPagedList<AuditResponse>>> GetPagedAudits(
        AuditPagingParameters pagingParameters,
        CancellationToken cancellationToken
    );
}

internal sealed record GetPagedAuditHandler(
    ILogger<GetPagedAuditHandler> logger,
    IAuditRepository repository,
    ExceptionNotificationService notificationService
) : IGetPagedAuditHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(GetPagedAuditHandler));

    public async ValueTask<ErrorOr<IPagedList<AuditResponse>>> GetPagedAudits(
        AuditPagingParameters pagingParameters,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(GetPagedAudits),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            var results = await repository.GetPagedAuditsAsync(pagingParameters, cancellationToken);
            return ErrorOrFactory.From(results);
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(
                logger,
                notificationService,
                activity: activity,
                message: StatusLabels.LoadFailed("Audit log"),
                entityName: "Audit log"
            );
        }
    }
}
