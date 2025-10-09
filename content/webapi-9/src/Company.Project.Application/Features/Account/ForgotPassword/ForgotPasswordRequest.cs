using System.ComponentModel.DataAnnotations;

namespace Company.Project.Application.Features.Account.ForgotPassword;

public sealed record ForgotPasswordRequest(string Username);
