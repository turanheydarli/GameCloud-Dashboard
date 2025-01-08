namespace GameCloud.Dashboard.Models.Requests;

public record RegisterDeveloperRequest(
    string Name,
    string Email,
    string Password,
    string ConfirmPassword);