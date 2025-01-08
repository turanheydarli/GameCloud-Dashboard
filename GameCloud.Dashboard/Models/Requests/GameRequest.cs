namespace GameCloud.Dashboard.Models.Requests;

public record GameRequest(
    string Name,
    string Description,
    Guid DeveloperId,
    bool IsEnabled = true,
    string? FirebaseApiKey = null,
    string? FirebaseProjectId = null
);