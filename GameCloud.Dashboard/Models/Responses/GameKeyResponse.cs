namespace GameCloud.Dashboard.Models.Responses;

public record GameKeyResponse(
    Guid Id,
    Guid GameId,
    string ApiKey,
    DateTime CreatedAt,
    GameKeyStatus Status
);