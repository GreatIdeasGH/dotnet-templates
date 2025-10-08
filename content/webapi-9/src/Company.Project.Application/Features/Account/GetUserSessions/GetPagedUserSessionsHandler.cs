namespace Company.Project.Application.Features.Account.GetUserSessions;

public interface IGetPagedUserSessionssHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiPagingResponse<UserSessionSummaryResponse>>> GetUserSessions(
        string userId,
        UserSessionsParameters pagingParameters,
        CancellationToken cancellationToken
    );
}

internal sealed record GetPagedUserSessionsHandler(
    ILogger<GetPagedUserSessionsHandler> logger,
    IUserSessionRepository repository
) : IGetPagedUserSessionssHandler
{
    private static readonly ActivitySource ActivitySource = new(
        nameof(GetPagedUserSessionsHandler)
    );

    public async ValueTask<ErrorOr<ApiPagingResponse<UserSessionSummaryResponse>>> GetUserSessions(
        string userId,
        UserSessionsParameters pagingParameters,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(GetUserSessions),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            var results = await repository.GetUserSessionsSummary(
                userId,
                pagingParameters,
                cancellationToken
            );

            var pagination = new PagedListMetaData(results);
            var paged = new ApiPagingResponse<UserSessionSummaryResponse>(results, pagination);

            return ErrorOrFactory.From(paged);
        }
        catch (TaskCanceledException exception)
        {
            return exception.LogCancelledTask(
                logger,
                activity: activity,
                item: nameof(GetPagedUserSessionsHandler)
            );
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(
                logger,
                activity: activity,
                message: StatusLabels.LoadFailed("User Sessions"),
                entityName: nameof(ApplicationUser)
            );
        }
    }
}
