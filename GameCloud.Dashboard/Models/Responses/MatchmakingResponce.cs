using System.Text.Json;

namespace GameCloud.Dashboard.Models.Responses;

public record MatchmakingResponse(
    Guid Id,
    Guid GameId,
    string Name,
    int MinPlayers,
    int MaxPlayers,
    TimeSpan TicketTTL,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MatchResponse(
    Guid Id,
    Guid GameId,
    string QueueName,
    string State,
    List<Guid> PlayerIds,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? UpdatedAt,
    JsonDocument? MatchData
);


public record MatchTicketResponse(
    Guid Id,
    Guid GameId,
    Guid PlayerId,
    string QueueName,
    string Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    Guid? MatchId,
    JsonDocument? CustomProperties
);