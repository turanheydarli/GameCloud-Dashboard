namespace GameCloud.Dashboard.Models.Requests;

public record DeveloperRequest(Guid Id, Guid UserId, string Name, string Email);