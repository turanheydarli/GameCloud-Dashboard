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

    #endregion
}