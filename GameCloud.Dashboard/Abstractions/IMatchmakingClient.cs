using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Refit;

namespace GameCloud.Dashboard.Abstractions;

[Headers("Content-Type: application/json")]
public interface IMatchmakingClient
{
    [Post("/api/v1/matchmaking/queues")]
    Task<MatchmakingResponse> CreateQueueAsync([Body] MatchQueueRequest request);

    [Get("/api/v1/matchmaking/queues/{queueId}")]
    Task<MatchmakingResponse> GetQueueAsync(Guid queueId);

    [Put("/api/v1/matchmaking/queues/{queueId}")]
    Task<MatchmakingResponse> UpdateQueueAsync(Guid queueId, [Body] MatchQueueRequest request);

    [Delete("/api/v1/matchmaking/queues/{queueId}")]
    Task DeleteQueueAsync(Guid queueId);

    [Post("/api/v1/matchmaking/tickets")]
    Task<MatchTicketResponse> EnqueuePlayerAsync([Body] EnqueuePlayerRequest request);

    [Delete("/api/v1/matchmaking/tickets/{ticketId}")]
    Task CancelTicketAsync(Guid ticketId);

    [Get("/api/v1/matchmaking/tickets/{ticketId}")]
    Task<MatchTicketResponse> GetTicketAsync(Guid ticketId);

    [Get("/api/v1/matchmaking/tickets/{ticketId}/match")]
    Task<MatchResponse> CheckMatchStatusAsync(Guid ticketId);

    [Get("/api/v1/matchmaking/matches/{matchId}")]
    Task<MatchResponse> GetMatchAsync(Guid matchId);

    [Put("/api/v1/matchmaking/matches/{matchId}/state")]
    Task<MatchResponse> UpdateMatchStateAsync(Guid matchId, [Body] MatchState state);

    [Delete("/api/v1/matchmaking/matches/{matchId}")]
    Task CancelMatchAsync(Guid matchId);

    [Post("/api/v1/matchmaking/matches/process")]
    Task<List<MatchResponse>> ProcessMatchmakingAsync([Query] Guid? queueId = null);
}