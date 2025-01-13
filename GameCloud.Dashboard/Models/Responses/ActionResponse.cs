using System.Text.Json;

namespace GameCloud.Dashboard.Models.Responses;

public record ActionResponse(
    Guid Id,
    Guid SessionId,
    Guid PlayerId,
    string ActionType,
    JsonDocument? Payload,
    JsonDocument? Result,
    DateTime ExecutedAt
)
{
    public string? UserName { get; set; }
}