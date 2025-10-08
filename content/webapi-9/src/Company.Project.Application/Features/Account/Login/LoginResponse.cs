using System.ComponentModel.DataAnnotations;

namespace Company.Project.Application.Features.Account.Login;

public sealed record LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
}

public sealed record LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
