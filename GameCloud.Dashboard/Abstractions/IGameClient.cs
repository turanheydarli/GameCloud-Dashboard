using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Refit;

namespace GameCloud.Dashboard.Abstractions;

public interface IGameClient
{
    #region Games

    [Get("/api/v1/games")]
    Task<PageableListResponse<GameResponse>> GetAllAsync([Query] PageableRequest request);

    [Get("/api/v1/games/{gameId}")]
    Task<GameResponse> GetAsync(Guid gameId);

    [Post("/api/v1/games")]
    Task<GameResponse> CreateAsync([Body] GameRequest request);

    [Put("/api/v1/games/{gameId}")]
    Task<GameResponse> UpdateAsync(Guid gameId, [Body] GameRequest request);

    [Get("/api/v1/games/{gameId}/details")]
    Task<GameDetailResponse> GetGameDetailsAsync(Guid gameId);

    [Delete("/api/v1/games/{gameId}")]
    Task DeleteAsync(Guid gameId);

    #endregion

    #region Game Keys

    [Get("/api/v1/games/{gameId}/keys")]
    Task<PageableListResponse<GameKeyResponse>> GetKeysAsync(Guid gameId, [Query] PageableRequest request);

    [Post("/api/v1/games/{gameId}/keys")]
    Task<GameKeyResponse> CreateKeyAsync(Guid gameId);

    [Delete("/api/v1/games/{gameId}/keys/{key}")]
    Task DeleteKeyAsync(Guid gameId, string key);
    
    [Get("/api/v1/games/{gameId}/keys/default")]
    Task<GameKeyResponse> GetOrCreateDefaultGameKey(Guid gameId);

    #endregion

    #region Game Images

    [Multipart]
    [Post("/api/v1/games/{gameId}/icon")]
    Task<ImageResponse> SetImageAsync(Guid gameId, StreamPart image);

    [Get("/api/v1/games/{gameId}/images")]
    Task<ImageResponse> GetImageDetailsAsync(Guid gameId);

    #endregion

    #region Functions

    [Post("/api/v1/games/{gameId}/functions")]
    Task<FunctionResponse> CreateFunctionAsync(Guid gameId, [Body] FunctionRequest request);

    [Get("/api/v1/games/{gameId}/functions/{functionId}")]
    Task<FunctionResponse> GetFunctionAsync(Guid gameId, Guid functionId);

    [Get("/api/v1/games/{gameId}/functions")]
    Task<PageableListResponse<FunctionResponse>> GetFunctionsAsync(Guid gameId, [Query] PageableRequest request);

    [Put("/api/v1/games/{gameId}/functions/{functionId}")]
    Task<FunctionResponse> UpdateFunctionAsync(Guid gameId, Guid functionId, [Body] FunctionRequest request);

    [Delete("/api/v1/games/{gameId}/functions/{functionId}")]
    Task DeleteFunctionAsync(Guid gameId, Guid functionId);

    [Put("/api/v1/games/{gameId}/functions/{functionId}/toggle")]
    Task ToggleFunctionAsync(Guid gameId, Guid functionId, bool isEnabled);

    #endregion

    #region Function Stats

    [Get("/api/v1/games/{gameId}/functions/{functionId}/stats")]
    Task<ActionStatsResponse> GetFunctionStatsAsync(
        Guid gameId,
        Guid functionId,
        [Query] DateTime? from = null,
        [Query] DateTime? to = null);

    [Get("/api/v1/games/{gameId}/functions/stats")]
    Task<ActionListStatsResponse> GetListFunctionStatsAsync(
        Guid gameId,
        [Query] DateTime? from = null,
        [Query] DateTime? to = null);

    #endregion


    #region Function Testing

    [Post("/api/v1/games/{gameId}/functions/{functionId}/test")]
    Task<ActionResponse> TestFunctionAsync(Guid gameId, Guid functionId, [Body] ActionRequest request,
        [Header("X-Game-Key")] string gameKey);

    [Post("/api/v1/games/{gameId}/functions/{functionId}/logs")]
    Task<PageableListResponse<ActionResponse>> GetFunctionLogsAsync(
        Guid gameId,
        Guid functionId,
        [Body] DynamicRequest? pageableRequest);

    #endregion

}