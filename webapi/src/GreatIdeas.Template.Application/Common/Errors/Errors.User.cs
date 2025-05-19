namespace GreatIdeas.Template.Application.Common.Errors;

public static class DomainUserErrors
{
    private const string _userNotFoundCode = "User.NotFound";
    private const string _userInActiveCode = "User.InActive";
    private const string _userNotConfirmedCode = "User.NotConfirmed";
    private const string _alreadyConfirmedCode = "User.AlreadyConfirmed";
    private const string _userCreationFailedCode = "User.CreationFailed";
    private const string _userUpdateFailedCode = "User.UpdateFailed";
    private const string _userAlreadyExistsCode = "User.Exists";
    private const string _roleNotFoundCode = "Role.NotFound";
    private const string _userInvalidCredentialsCode = "User.InvalidCredentials";

    public static Error Exists(string description) =>
        Error.Conflict(code: _userAlreadyExistsCode, description: $"{description}");

    public static Error EmailExists(string email) =>
        Error.Conflict(
            code: _userAlreadyExistsCode,
            description: $"Email: '{email}' already exists."
        );

    public static Error PhoneNumberExists(string phoneNumber) =>
        Error.Conflict(
            code: _userAlreadyExistsCode,
            description: $"PhoneNumber: '{phoneNumber}' already exists."
        );

    public static Error InvalidLoginCredentials { get; } =
        Error.Custom(
            type: 401,
            code: _userInvalidCredentialsCode,
            description: "Wrong username or password"
        );

    public static Error UserNotFound { get; } =
        Error.NotFound(code: _userNotFoundCode, description: "User account not found.");

    public static Error InActive { get; } =
        Error.Custom(
            type: 401,
            code: _userInActiveCode,
            description: "InActive user, please contact your administrator."
        );

    public static Error NotConfirmed { get; } =
        Error.Custom(
            type: 401,
            code: _userNotConfirmedCode,
            description: "User account is not confirmed. Please confirm your email address."
        );

    public static Error AlreadyConfirmed { get; } =
        Error.Conflict(
            code: _alreadyConfirmedCode,
            description: "User account is already confirmed. Please login to continue."
        );

    public static Error RoleNotFound { get; } =
        Error.NotFound(code: _roleNotFoundCode, description: "Role not found.");

    public static Error RefreshTokenUpdateError(string error) =>
        Error.Custom(type: 401, code: "User.RefreshTokenUpdateFailed", description: error);

    public static Error PasswordChangeFailed(string error) =>
        Error.Failure(code: "User.PasswordChangeFailed", description: error);

    public static Error ConfirmEmailFailed(string error) =>
        Error.Failure(code: "User.EmailConfirmationFailed", description: error);

    public static Error DeleteFailed(string errorMessage) =>
        Error.Failure(code: "User.DeleteFailed", description: errorMessage);

    public static Error CreationFailed(string error) =>
        Error.Failure(code: _userCreationFailedCode, description: error);

    public static Error UpdateFailed(string error) =>
        Error.Failure(code: _userUpdateFailedCode, description: error);

    public static Error SubscriptionExpired =>
        Error.Custom(
            type: 401,
            code: "User.SubscriptionExpired",
            description: "Subscription expired. Please renew your subscription."
        );
}
