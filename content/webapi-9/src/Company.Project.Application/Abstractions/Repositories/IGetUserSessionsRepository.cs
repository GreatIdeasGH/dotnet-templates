using System.Net;

using Company.Project.Application.Features.Account.GetUserSessions;

using Company.Project.Domain.Entities;

namespace Company.Project.Application.Abstractions.Repositories;

public interface IUserSessionRepository
{
    ValueTask<IPagedList<UserSessionResponse>> GetUserSessions(
        string userId,
        UserSessionsParameters pagingParameters,
        CancellationToken cancellationToken
    );

    ValueTask<IPagedList<UserSessionSummaryResponse>> GetUserSessionsSummary(
        string userId,
        UserSessionsParameters pagingParameters,
        CancellationToken cancellationToken
    );

    ValueTask<UserSession> CreateSessionAsync(
        string userId,
        IPAddress? ipAddress,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    );

    ValueTask<UserSession> CreateSessionAsync(
        string userId,
        string userAgent,
        IPAddress ipAddress,
        CancellationToken cancellationToken = default
    );

    ValueTask UpdateLastActivityAsync(string userId, CancellationToken cancellationToken = default);

    ValueTask LogoutSessionAsync(
        string userId,
        string? sessionToken = null,
        CancellationToken cancellationToken = default
    );

    ValueTask LogoutAllSessionsAsync(string userId, CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<UserSession>> GetActiveSessionsAsync(
        string userId,
        CancellationToken cancellationToken = default
    );

    ValueTask<IEnumerable<UserSession>> GetUserSessionHistoryAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );
}
