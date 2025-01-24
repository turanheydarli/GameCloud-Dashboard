using System.Text.Json;

namespace GameCloud.Dashboard.Models.Requests;

public record ActionRequest(
    Guid SessionId,
    string ActionType,
    JsonElement Payload,
    string ClientVersion = "unknown",
    string ClientPlatform = "WEB_DASHBOARD"
);
