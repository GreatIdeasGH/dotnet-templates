using Company.Project.Application.Common.Errors;

namespace Company.Project.Application.Features.Audits;

public interface IGetAuditLogByIdHandler : IApplicationHandler
{
    ValueTask<ErrorOr<AuditDetailResponse>> GetAuditLogById(
        Guid id,
        CancellationToken cancellationToken
    );
}

internal sealed record GetAuditLogByIdHandler(
    IAuditRepository repository,
    ILogger<GetAuditLogByIdHandler> logger,
    ExceptionNotificationService notificationService
) : IGetAuditLogByIdHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(GetAuditLogByIdHandler));

    public async ValueTask<ErrorOr<AuditDetailResponse>> GetAuditLogById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(GetAuditLogById),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            var results = await repository.GetByIdAsync(id, cancellationToken);
            if (results == null)
            {
                var error = DomainErrors.NotFound(nameof(AuditTrail));
                OtelConstants.AddErrorEvent(activity, error);
                return error;
            }

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
