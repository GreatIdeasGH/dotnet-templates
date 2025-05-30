﻿namespace GreatIdeas.Template.Application.Features.Account.ChangePassword;

public interface IChangePasswordHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> ChangePassword(string userId, ChangePasswordRequest request, CancellationToken cancellationToken);
}

internal sealed class ChangePasswordHandler(
    IUserRepository userRepository,
    ILogger<ChangePasswordHandler> logger) : IChangePasswordHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(ChangePasswordHandler));

    public async ValueTask<ErrorOr<ApiResponse>> ChangePassword(string userId, ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.CreateActivity(nameof(ChangePassword), ActivityKind.Server);
        activity?.Start();

        try
        {
            var result = await userRepository.ChangePassword(userId, request, cancellationToken);
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
                userId,
                "Could not change user password");
        }
    }
}