namespace Company.Project.Application.Common.Errors;

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

    public static Error InvalidLoginCredentials =>
        Error.Failure(code: _userInvalidCredentialsCode, description: "Wrong username or password");

    public static Error UserNotFound =>
        Error.NotFound(code: _userNotFoundCode, description: "User account not found.");

    public static Error InActive =>
        Error.Failure(
            code: _userInActiveCode,
            description: "InActive user, please contact your administrator."
        );

    public static Error NotConfirmed =>
        Error.Failure(
            code: _userNotConfirmedCode,
            description: "User account is not confirmed. Please confirm your email address."
        );

    public static Error AlreadyConfirmed =>
        Error.Conflict(
            code: _alreadyConfirmedCode,
            description: "User account is already confirmed. Please login to continue."
        );

    public static Error RoleNotFound =>
        Error.NotFound(code: _roleNotFoundCode, description: "Role not found.");

    public static Error RefreshTokenUpdateError(string errorMessage) =>
        Error.Failure(code: "User.RefreshTokenUpdateFailed", description: errorMessage);

    public static Error PasswordChangeFailed(string errorMessage) =>
        Error.Failure(code: "User.PasswordChangeFailed", description: errorMessage);

    public static Error ConfirmEmailFailed(string errorMessage) =>
        Error.Failure(code: "User.EmailConfirmationFailed", description: errorMessage);

    public static Error DeleteFailed(string errorMessage) =>
        Error.Failure(code: "User.DeleteFailed", description: errorMessage);

    public static Error CreationFailed(string errorMessage) =>
        Error.Failure(code: _userCreationFailedCode, description: errorMessage);

    public static Error UpdateFailed(string errorMessage) =>
        Error.Failure(code: _userUpdateFailedCode, description: errorMessage);

    public static Error SubscriptionExpired =>
        Error.Failure(
            code: "User.SubscriptionExpired",
            description: "Subscription expired. Please renew your subscription."
        );
}
