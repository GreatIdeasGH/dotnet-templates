using System.Net;
using Company.Project.Application.Features.Account.GetUserSessions;
using Company.Project.Infrastructure.Data;

namespace Company.Project.Infrastructure.Repositories;

internal sealed class UserSessionRepository(
    ApplicationDbContext dbContext,
    IIpGeolocationService ipGeolocationService,
    ILogger<UserSessionRepository> logger
) : IUserSessionRepository
{
    public async ValueTask<IPagedList<UserSessionResponse>> GetUserSessions(
        string userId,
        UserSessionsParameters pagingParameters,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext.UserSessions.AsNoTracking().Where(s => s.UserId == userId);

        // Filter by active status
        if (pagingParameters.ActiveOnly.HasValue)
        {
            query = query.Where(s => s.IsActive == pagingParameters.ActiveOnly.Value);
        }

        // Apply pagination and ordering
        var sessions = await query
            .Select(s => new UserSessionResponse
            {
                UserSessionId = s.Id,
                UserId = s.UserId,
                IpAddress = s.IpAddress != null ? s.IpAddress.ToString() : null,
                UserAgent = s.UserAgent,
                DeviceInfo = s.DeviceInfo,
                Location = s.Location,
                Country = s.Country,
                City = s.City,
                Region = s.Region,
                Latitude = s.Latitude!.Value,
                Longitude = s.Longitude!.Value,
                Timezone = s.Timezone,
                Organization = s.Organization,
                LoginAt = s.LoginAt,
                LogoutAt = s.LogoutAt,
                LastActivityAt = s.LastActivityAt,
                IsActive = s.IsActive,
                SessionToken = s.SessionToken,
            })
            .OrderByDescending(s => s.LoginAt)
            .TagWith("GetUserSessions")
            .ToPagedListAsync(pagingParameters.PageNumber, pagingParameters.PageSize);

        return sessions;
    }

    public async ValueTask<IPagedList<UserSessionSummaryResponse>> GetUserSessionsSummary(
        string userId,
        UserSessionsParameters pagingParameters,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext.UserSessions.AsNoTracking().Where(s => s.UserId == userId);

        // Filter by active status
        if (pagingParameters.ActiveOnly.HasValue)
        {
            query = query.Where(s => s.IsActive == pagingParameters.ActiveOnly.Value);
        }

        // Apply pagination and ordering
        var sessions = await query
            .Select(s => new UserSessionSummaryResponse
            {
                UserSessionId = s.Id,
                UserId = s.UserId,
                UserAgent = s.UserAgent,
                IpAddress = s.IpAddress != null ? s.IpAddress.ToString() : null,
                LoginAt = s.LoginAt,
                LogoutAt = s.LogoutAt,
                LastActivityAt = s.LastActivityAt,
                IsActive = s.IsActive,
                SessionToken = s.SessionToken,
            })
            .OrderByDescending(s => s.LoginAt)
            .TagWith(nameof(GetUserSessionsSummary))
            .ToPagedListAsync(pagingParameters.PageNumber, pagingParameters.PageSize);

        return sessions;
    }

    public async ValueTask<UserSession> CreateSessionAsync(
        string userId,
        IPAddress? ipAddress,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Get geolocation information
            var geoInfo = await ipGeolocationService.GetLocationAsync(cancellationToken);

            // Parse device info from user agent (basic parsing)
            var deviceInfo = ParseDeviceInfo(userAgent);

            // Create new session
            var session = UserSession.CreateNew(
                userId: userId,
                ipAddress: ipAddress,
                userAgent: userAgent,
                deviceInfo: deviceInfo,
                location: geoInfo?.FullLocation,
                country: geoInfo?.Country,
                city: geoInfo?.City,
                region: geoInfo?.Region,
                latitude: geoInfo?.Latitude,
                longitude: geoInfo?.Longitude,
                timezone: geoInfo?.Timezone,
                organization: geoInfo?.Organization
            );

            // Add to database
            await dbContext.UserSessions.AddAsync(session, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created new session {SessionId} for user {UserId} from IP {IpAddress} ({Location})",
                session.SessionToken,
                userId,
                ipAddress,
                geoInfo?.FullLocation ?? "Unknown"
            );

            return session;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async ValueTask<UserSession> CreateSessionAsync(
        string userId,
        string userAgent,
        IPAddress ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Create new session
            var session = UserSession.CreateNew(
                userId: userId,
                userAgent: userAgent,
                ipAddress: ipAddress
            );

            // Add to database
            await dbContext.UserSessions.AddAsync(session, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created new session {SessionId} for user {UserId} from IP {IpAddress}",
                session.SessionToken,
                userId,
                ipAddress
            );

            return session;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async ValueTask UpdateLastActivityAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var activeSessions = await dbContext
                .UserSessions.AsNoTracking()
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var session in activeSessions)
            {
                session.UpdateLastActivity();
            }

            if (activeSessions.Count > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating last activity for user {UserId}", userId);
        }
    }

    public async ValueTask LogoutSessionAsync(
        string userId,
        string? sessionToken = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            IQueryable<UserSession> query = dbContext.UserSessions.Where(s =>
                s.UserId == userId && s.IsActive
            );

            if (!string.IsNullOrEmpty(sessionToken))
            {
                query = query.Where(s => s.SessionToken == sessionToken);
            }

            var sessions = await query.ToListAsync(cancellationToken);

            foreach (var session in sessions)
            {
                session.MarkAsLoggedOut();
            }

            if (sessions.Count > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation(
                    "Logged out {Count} session(s) for user {UserId}",
                    sessions.Count,
                    userId
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging out session for user {UserId}", userId);
        }
    }

    public async ValueTask LogoutAllSessionsAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        await LogoutSessionAsync(userId, sessionToken: null, cancellationToken);
    }

    public async ValueTask<IEnumerable<UserSession>> GetActiveSessionsAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .UserSessions.AsNoTracking()
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(cancellationToken);
    }

    public async ValueTask<IEnumerable<UserSession>> GetUserSessionHistoryAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .UserSessions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LoginAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    private static string? ParseDeviceInfo(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        return userAgent switch
        {
            var ua when ua.Contains("Mobile", StringComparison.OrdinalIgnoreCase) => "Mobile",
            var ua when ua.Contains("Tablet", StringComparison.OrdinalIgnoreCase) => "Tablet",
            var ua when ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) => "iPhone",
            var ua when ua.Contains("iPad", StringComparison.OrdinalIgnoreCase) => "iPad",
            var ua when ua.Contains("Android", StringComparison.OrdinalIgnoreCase) => "Android",
            var ua when ua.Contains("Windows", StringComparison.OrdinalIgnoreCase) => "Windows",
            var ua when ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) => "Mac",
            var ua when ua.Contains("Linux", StringComparison.OrdinalIgnoreCase) => "Linux",
            _ => "Desktop",
        };
    }
}
