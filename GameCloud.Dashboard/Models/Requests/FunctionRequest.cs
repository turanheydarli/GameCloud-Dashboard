namespace GameCloud.Dashboard.Models.Requests;

public record FunctionRequest(
    Guid Id,
    Guid GameId,
    string Name,
    string? Description,
    string ActionType,
    string Endpoint,
    bool IsEnabled,
    TimeSpan Timeout,
    Dictionary<string, string>? Headers,
    int MaxRetries
);