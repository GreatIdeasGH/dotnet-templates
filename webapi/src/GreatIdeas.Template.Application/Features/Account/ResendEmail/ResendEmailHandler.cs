namespace GreatIdeas.Template.Application.Features.Account.ResendEmail;

public interface IResendEmailHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> ResendEmail(ResendEmailRequest request, CancellationToken cancellationToken);
}

internal sealed class ResendEmailHandler(
    ILogger<ResendEmailHandler> logger,
    IUserRepository userRepository,
    IPublishEndpoint publishEndpoint
) : IResendEmailHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(ResendEmailHandler));

    public async ValueTask<ErrorOr<ApiResponse>> ResendEmail(ResendEmailRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.CreateActivity(nameof(ResendEmail), ActivityKind.Server);
        activity?.Start();

        try
        {
            // Get Confirmation code
            var response = await userRepository.ResendConfirmEmail(request, cancellationToken);
            if (response.IsError)
            {
                return response.Errors;
            }

            // publish event
            await publishEndpoint.Publish(new ResendEmailEvent(
                response.Value.UserId,
                response.Value.Email,
                response.Value.VerificationCode), cancellationToken);

            return new ApiResponse("Email confirmation sent successfully. Please check your email.");
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(logger,
                activity,
                request.Email,
                "Could not resend email confirmation");
        }
    }
}