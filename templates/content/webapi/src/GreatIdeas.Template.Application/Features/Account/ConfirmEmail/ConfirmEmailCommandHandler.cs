namespace GreatIdeas.Template.Application.Features.Account.ConfirmEmail;

public interface IConfirmEmailHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> ConfirmEmail(ConfirmEmailResponse request);
}

internal sealed class ConfirmEmailHandler(
    ILogger<ConfirmEmailHandler> logger,
    IUserRepository userRepository) : IConfirmEmailHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(ConfirmEmailHandler));

    public async ValueTask<ErrorOr<ApiResponse>> ConfirmEmail(ConfirmEmailResponse request)
    {
        using var activity = ActivitySource.CreateActivity(nameof(ConfirmEmail), ActivityKind.Server);
        activity?.Start();

        try
        {
            var result = await userRepository.ConfirmEmail(request, CancellationToken.None);
            if (result.IsError)
            {
                return result.Errors;
            }

            return new ApiResponse(result.Value);
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(logger,
                activity,
                request.UserId,
                exception.Message);
        }
    }
}