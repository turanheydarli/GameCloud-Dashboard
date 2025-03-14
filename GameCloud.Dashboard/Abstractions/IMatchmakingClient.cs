using System.Text.Json;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Refit;

namespace GameCloud.Dashboard.Abstractions;

[Headers("Content-Type: application/json")]
public interface IMatchmakingClient
{
    [Post("/api/v1/matchmaking/queues")]
    Task<MatchmakingResponse> CreateQueueAsync([Header("X-Game-Key")] string gameKey, [Body] MatchQueueRequest request);

    [Get("/api/v1/matchmaking/queues/{queueId}")]
    Task<MatchmakingResponse> GetQueueAsync([Header("X-Game-Key")] string gameKey, Guid queueId);

    [Put("/api/v1/matchmaking/queues/{queueId}")]
    [Headers("X-Game-Key: {gameKey}")]
    Task<MatchmakingResponse> UpdateQueueAsync([Header("X-Game-Key")] string gameKey, Guid queueId,
        [Body] MatchQueueRequest request);

    [Delete("/api/v1/matchmaking/queues/{queueId}")]
    Task DeleteQueueAsync([Header("X-Game-Key")] string gameKey, Guid queueId);

    [Post("/api/v1/matchmaking/tickets")]
    Task<MatchTicketResponse> EnqueuePlayerAsync([Header("X-Game-Key")] string gameKey,
        [Body] EnqueuePlayerRequest request);

    [Delete("/api/v1/matchmaking/tickets/{ticketId}")]
    Task CancelTicketAsync([Header("X-Game-Key")] string gameKey, Guid ticketId);

    [Get("/api/v1/matchmaking/tickets/{ticketId}")]
    Task<MatchTicketResponse> GetTicketAsync([Header("X-Game-Key")] string gameKey, Guid ticketId);

    [Get("/api/v1/matchmaking/tickets/{ticketId}/match")]
    Task<MatchResponse> CheckMatchStatusAsync([Header("X-Game-Key")] string gameKey, Guid ticketId);

    [Get("/api/v1/matchmaking/matches/{matchId}")]
    Task<MatchResponse> GetMatchAsync([Header("X-Game-Key")] string gameKey, Guid matchId);

    [Put("/api/v1/matchmaking/matches/{matchId}/state")]
    Task<MatchResponse> UpdateMatchStateAsync([Header("X-Game-Key")] string gameKey, Guid matchId,
        [Body] MatchState state);

    [Delete("/api/v1/matchmaking/matches/{matchId}")]
    Task CancelMatchAsync(string gameKey, Guid matchId);

    [Post("/api/v1/matchmaking/matches/process")]
    Task<List<MatchResponse>> ProcessMatchmakingAsync([Header("X-Game-Key")] string gameKey,
        [Query] Guid? queueId = null);

    [Get("/api/v1/matchmaking/queues")]
    Task<PageableListResponse<MatchmakingResponse>> GetQueuesAsync([Header("X-Game-Key")] string gameKey,
        [Query] string? search = null,
        [Query] int pageIndex = 0, [Query] int pageSize = 10);

    [Post("/api/v1/matchmaking/queues/{queueId}/toggle")]
    Task<QueueToggleResponse> ToggleQueueAsync([Header("X-Game-Key")] string gameKey, Guid queueId,
        [Body] ToggleQueueRequest request);

    [Get("/api/v1/matchmaking/stats")]
    Task<MatchmakingStatsResponse> GetStatsAsync([Header("X-Game-Key")] string gameKey,
        [Query] List<Guid>? queueIds = null,
        [Query] string timeRange = "24h");

    [Get("/api/v1/matchmaking/queues/{queueId}/activity")]
    Task<QueueActivityResponse> GetQueueActivityAsync([Header("X-Game-Key")] string gameKey, Guid queueId,
        [Query] string timeRange = "24h");

    [Get("/api/v1/matchmaking/queues/{queueId}/matches")]
    Task<PageableListResponse<MatchResponse>> GetQueueMatchesAsync([Header("X-Game-Key")] string gameKey, Guid queueId,
        [Query] string? status = null, [Query] int pageIndex = 0, [Query] int pageSize = 10);

    [Get("/api/v1/matchmaking/queues/{queueId}/tickets")]
    Task<PageableListResponse<MatchTicketResponse>> GetQueueTicketsAsync([Header("X-Game-Key")] string gameKey,
        Guid queueId,
        [Query] int pageIndex = 0, [Query] int pageSize = 10);

    [Get("/api/v1/matchmaking/queues/{queueId}/functions")]
    Task<QueueFunctionsResponse> GetQueueFunctionsAsync([Header("X-Game-Key")] string gameKey, Guid queueId);

    [Put("/api/v1/matchmaking/queues/{queueId}/functions")]
    Task UpdateQueueFunctionsAsync([Header("X-Game-Key")] string gameKey, Guid queueId,
        [Body] UpdateQueueFunctionsRequest request);

    [Post("/api/v1/matchmaking/queues/{queueId}/test")]
    Task<QueueTestResponse> TestQueueAsync([Header("X-Game-Key")] string gameKey, Guid queueId,
        [Body] QueueTestRequest request);

    [Get("/api/v1/matchmaking/queues/{queueId}/dashboard")]
    Task<QueueDashboardResponse> GetQueueDashboardAsync([Header("X-Game-Key")] string gameKey, Guid queueId);

    [Get("/api/v1/matchmaking/queues/{queueId}/rules")]
    Task<JsonDocument> GetQueueRulesAsync([Header("X-Game-Key")] string gameKey, Guid queueId);

    [Put("/api/v1/matchmaking/queues/{queueId}/rules")]
    Task UpdateQueueRulesAsync([Header("X-Game-Key")] string gameKey, Guid queueId, [Body] JsonDocument rules);
}