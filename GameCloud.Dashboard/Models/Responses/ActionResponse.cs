using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCloud.Dashboard.Models.Responses;

public record ActionResponse(
    Guid Id,
    Guid SessionId,
    string PlayerId,
    Guid FunctionId,
    string ActionType,
    DateTime CreatedAt,
    DateTime StartedAt,
    DateTime CompletedAt,
    double ExecutionTimeMs,
    double TotalLatencyMs,
    FunctionStatus Status,
    string? ErrorCode,
    string? ErrorMessage,
    int RetryCount,
    JsonDocument? Payload,
    JsonDocument? Result,
    int PayloadSizeBytes,
    int ResultSizeBytes,
    IDictionary<string, string>? Metadata
);

public record ActionStatsResponse(
    Guid FunctionId,
    string ActionType,
    int TotalExecutions,
    int SuccessfulExecutions,
    int FailedExecutions,
    double SuccessRate,
    double AverageExecutionTimeMs,
    double AverageLatencyMs,
    double P95LatencyMs,
    double P99LatencyMs,
    long TotalPayloadBytes,
    long TotalResultBytes,
    double AveragePayloadSizeBytes,
    double AverageResultSizeBytes,
    DateTime? LastExecutedAt,
    string? LastErrorCode,
    string? LastErrorMessage,
    IReadOnlyList<ErrorStat> TopErrors,
    DateTime WindowStart,
    DateTime WindowEnd
);

public record ErrorStat(
    string Code,
    string Message,
    int Count,
    DateTime LastOccurred,
    double PercentageOfErrors
);

public record ActionListStatsResponse(
    Guid GameId,
    int TotalFunctions,
    int ActiveFunctions,
    int TotalExecutions,
    double OverallSuccessRate,
    double AverageExecutionTimeMs,
    double AverageLatencyMs,
    double TotalInboundTrafficMB,
    double TotalOutboundTrafficMB,
    IReadOnlyList<ActionStatsResponse> FunctionStats,
    DateTime WindowStart,
    DateTime WindowEnd
);

public record DateTimeRange(DateTime From, DateTime To)
{
    public static DateTimeRange Last24Hours =>
        new(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow);

    public static DateTimeRange LastWeek =>
        new(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

    public static DateTimeRange LastMonth =>
        new(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

    public static DateTimeRange Today =>
        new(DateTime.UtcNow.Date, DateTime.UtcNow);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FunctionStatus
{
    Success,
    Failed,
    PartialSuccess,
    Pending
}