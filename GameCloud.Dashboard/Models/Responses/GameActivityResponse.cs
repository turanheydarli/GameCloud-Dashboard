namespace GameCloud.Dashboard.Models.Responses;

public record GameActivityResponse(
    string EventType,
    DateTime Timestamp,
    string Details
);