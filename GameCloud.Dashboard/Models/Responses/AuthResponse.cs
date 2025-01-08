namespace GameCloud.Dashboard.Models.Responses;

public record AuthResponse(
    Guid UserId,
    string PlayerId,
    string Email,
    string Token,
    DateTime ExpiresAt
);