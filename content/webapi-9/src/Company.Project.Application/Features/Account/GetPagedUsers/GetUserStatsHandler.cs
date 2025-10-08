using Company.Project.Application.Features.Account.GetAccount;

namespace Company.Project.Application.Features.Account.GetPagedUsers;

public interface IGetUserStatsHandler : IApplicationHandler
{
    ValueTask<ErrorOr<UserQueryStats>> GetUserStats(CancellationToken cancellationToken);
}

internal sealed record GetUserStatsHandler(
    ILogger<GetUserStatsHandler> logger,
    IUserRepository repository
) : IGetUserStatsHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(GetUserStatsHandler));

    public async ValueTask<ErrorOr<UserQueryStats>> GetUserStats(
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(GetPagedUsers),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            var results = await repository.CountAdmins();
            return new UserQueryStats { TotalAdmins = results.Value };
        }
        catch (TaskCanceledException exception)
        {
            return exception.LogCancelledTask(
                logger,
                activity: activity,
                item: nameof(GetPagedUsersHandler)
            );
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(
                logger,
                activity: activity,
                message: StatusLabels.LoadFailed("Accounts"),
                entityName: nameof(ApplicationUser)
            );
        }
    }
}
