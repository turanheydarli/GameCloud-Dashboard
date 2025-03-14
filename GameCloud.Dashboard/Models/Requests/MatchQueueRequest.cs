using System.Text.Json;

namespace GameCloud.Dashboard.Models.Requests;

public record MatchQueueRequest(
    Guid Id,
    Guid GameId,
    string Name,
    int MinPlayers,
    int MaxPlayers,
    TimeSpan TicketTTL,
    MatchingCriteria Criteria);
    
    
public record OfflineMatchRequest(
    string? QueueName,
    MatchingCriteria? Criteria 
);

public record FindMatchRequest(
    string QueueName,
    JsonDocument? CustomProperties = null
);
public record UpdateQueueFunctionsRequest(
    Guid? InitializeFunctionId,
    Guid? TransitionFunctionId,
    Guid? LeaveFunctionId,
    Guid? EndFunctionId
);

public record QueueTestRequest(
    int Players,
    JsonDocument Properties
);

public record MatchActionRequest(
    string Type,
    JsonDocument Data,
    Guid? NextPlayerId = null,
    DateTime? NextDeadline = null
);

public record MatchQueueRulesRequest(
    JsonDocument Rules
);

public record ToggleQueueRequest(
    bool IsEnabled
);

public record CreateTicketRequest(
    string QueueName,
    JsonDocument? Properties = null
);

public record UpdatePresenceRequest(
    string SessionId,
    PresenceStatus Status,
    JsonDocument? Meta = null
);

public enum PresenceStatus
{
    Online,
    Away,
    Offline
}

public record MatchStateUpdateRequest(
    JsonDocument PlayerStates,
    JsonDocument MatchState,
    Guid? NextPlayerId = null,
    DateTime? NextDeadline = null
);

public record EndMatchRequest(
    JsonDocument? FinalState = null
);

public record LeaveMatchRequest(
    string? Reason = null
);

public enum TicketStatus
{
    /// <summary>
    /// Ticket is created and waiting in queue
    /// </summary>
    Queued = 0,

    /// <summary>
    /// Potential match is found, validating requirements
    /// </summary>
    Matching = 1,

    /// <summary>
    /// Match is found and waiting for player response
    /// </summary>
    MatchFound = 2,

    /// <summary>
    /// Player accepted the match
    /// </summary>
    Accepted = 3,

    /// <summary>
    /// Player declined the match
    /// </summary>
    Declined = 4,

    /// <summary>
    /// Ticket is cancelled by player
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Ticket expired due to timeout
    /// </summary>
    Expired = 6,

    /// <summary>
    /// Match is successfully created
    /// </summary>
    Matched = 7,

    /// <summary>
    /// Error occurred during matchmaking
    /// </summary>
    Error = 8
}

public enum MatchState
{
    /// <summary>
    /// Match is created but not all players accepted yet
    /// </summary>
    Created = 0,

    /// <summary>
    /// All players accepted, match is ready to start
    /// </summary>
    Ready = 1,

    /// <summary>
    /// Match is in progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Waiting for next player's turn
    /// </summary>
    WaitingTurn = 3,

    /// <summary>
    /// Match finished normally
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Match cancelled due to player decline/timeout
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Match abandoned by player(s)
    /// </summary>
    Abandoned = 6,

    /// <summary>
    /// Match expired due to inactivity
    /// </summary>
    Expired = 7,

    /// <summary>
    /// Error occurred during match
    /// </summary>
    Error = 8
}

public record MatchingCriteria(List<AttributeCriteria> Attributes);

public record AttributeCriteria(
    string Collection,
    string Key,
    string Operator,
    RangeCriteria? Range = null);

public record RangeCriteria(
    double ExpansionRate,
    double? MaxExpansion);

public record UpdateMatchStateRequest(MatchState NewState);

public record EnqueuePlayerRequest(
    Guid GameId,
    Guid PlayerId,
    string QueueName,
    JsonDocument? CustomProperties
);
