using GreatIdeas.Template.Application.Abstractions.Repositories;
using GreatIdeas.Template.Application.Common.Params;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Domain.Entities;

namespace GreatIdeas.Template.Application.Features.Account.GetPagedUsers;

public interface IGetPagedUsersHandler : IApplicationHandler
{
    ValueTask<ErrorOr<IPagedList<UserAccountResponse>>> GetPagedUsers(
        PagingParameters pagingParameters,
        CancellationToken cancellationToken
    );
}

internal sealed record GetPagedUsersHandler(
    ILogger<GetPagedUsersHandler> logger,
    IUserRepository repository
) : IGetPagedUsersHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(GetPagedUsersHandler));

    public async ValueTask<ErrorOr<IPagedList<UserAccountResponse>>> GetPagedUsers(
        PagingParameters pagingParameters,
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
            var results = await repository.GetPagedUsersAsync(pagingParameters, cancellationToken);
            return ErrorOrFactory.From(results);
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
