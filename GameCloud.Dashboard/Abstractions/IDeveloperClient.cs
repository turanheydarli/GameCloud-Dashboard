using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Refit;

namespace GameCloud.Dashboard.Abstractions;

[Headers("Content-Type: application/json")]
public interface IDeveloperClient
{
    [Get("/api/v1/developers")]
    Task<PageableListResponse<DeveloperResponse>> GetDevelopersAsync([Query] PageableRequest request);

    [Get("/api/v1/developers/{id}")]
    Task<DeveloperResponse> GetDeveloperByIdAsync(Guid id);

    [Post("/api/v1/developers")]
    Task<DeveloperResponse> RegisterDeveloperAsync([Body] RegisterDeveloperRequest request);

    [Post("/api/v1/developers/login")]
    Task<AuthResponse> LoginDeveloperAsync([Body] LoginDeveloperRequest request);

    [Get("/api/v1/developers/me")]
    [Headers("Authorization: Bearer")]
    Task<DeveloperResponse> GetMeAsync();

    [Put("/api/v1/developers/me")]
    [Headers("Authorization: Bearer")]
    Task<DeveloperResponse> UpdateMeAsync([Body] DeveloperRequest request);
}
