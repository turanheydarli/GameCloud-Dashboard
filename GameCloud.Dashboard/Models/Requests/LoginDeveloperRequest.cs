namespace GameCloud.Dashboard.Models.Requests;

public record LoginDeveloperRequest(
    string Email,
    string Password);