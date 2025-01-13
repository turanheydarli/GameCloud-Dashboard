namespace GameCloud.Dashboard.Models.Responses;

public record GameResponse(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl,
    Guid DeveloperId,
    Guid ImageId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public enum GameKeyStatus
{
    Active = 1,
    Revoked = 2,
}