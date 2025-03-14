using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCloud.Dashboard.Models.Responses;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QueueType
{
    [JsonPropertyName("turnBased")] TurnBased,
    [JsonPropertyName("simultaneous")] Simultaneous,
    [JsonPropertyName("asynchronous")] Asynchronous
}

public record MatchmakingResponse(
    Guid Id,
    Guid GameId,
    string Name,
    int MinPlayers,
    int MaxPlayers,
    TimeSpan TicketTTL,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    QueueType QueueType,
    bool IsEnabled
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

public record QueueToggleResponse(
    bool Success,
    bool IsEnabled,
    Guid QueueId
);

public record QueueActivityResponse(
    List<string> Labels,
    List<int> Matches,
    List<int> Players
);

public record QueueFunctionInfo(
    Guid Id,
    string Name,
    string ActionType
);

public record QueueFunctionsResponse(
    QueueFunctionInfo? Initialize,
    QueueFunctionInfo? Transition,
    QueueFunctionInfo? Leave,
    QueueFunctionInfo? End
);

public record QueueStatsResponse(
    Guid QueueId,
    int ActiveMatches,
    int WaitingPlayers,
    double AvgWaitTimeSeconds,
    int TotalPlayers,
    double AverageWaitTime,
    string AverageWaitTimeTrend,
    double AverageWaitTimeChange,
    double MatchmakingSuccessRate
);

public record MatchmakingStatsResponse(
    List<QueueStatsResponse> QueueStats
);

public record QueueDashboardResponse(
    MatchmakingResponse Queue,
    QueueStatsResponse Stats,
    QueueFunctionsResponse Functions
);

public record TestPlayer(
    Guid Id,
    JsonDocument Properties
);

public record QueueTestResponse(
    bool Success,
    Guid MatchId,
    List<TestPlayer> Players,
    double Duration
);
