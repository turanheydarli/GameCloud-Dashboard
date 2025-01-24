using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games.Matchmaking;

public class MatchmakingModel : PageModel
{
    private readonly IMatchmakingClient _matchmakingClient;
    private readonly IGameClient _gameClient;
    private readonly ILogger<MatchmakingModel> _logger;

    public MatchmakingModel(
        IMatchmakingClient matchmakingClient,
        IGameClient gameClient,
        ILogger<MatchmakingModel> logger)
    {
        _matchmakingClient = matchmakingClient;
        _gameClient = gameClient;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)] 
    public Guid GameId { get; set; }

    [BindProperty(SupportsGet = true)] 
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)] 
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)] 
    public int Page { get; set; } = 0;

    public GameResponse Game { get; private set; }
    public PageableListResponse<MatchmakingResponse> Queues { get; private set; }
    public string? ErrorMessage { get; private set; }
    // public MatchmakingStatsResponse Stats { get; private set; }

    public async Task<IActionResult> OnGetAsync([FromRoute] Guid gameId)
    {
        try
        {
            var getGameTask = _gameClient.GetAsync(gameId);
            // var getQueuesTask = _matchmakingClient.GetQueuesAsync(new PageableRequest
            // {
            //     PageIndex = Page,
            //     PageSize = PageSize,
            //     Search = Search,
            //     IsAscending = false
            // });
            // var getStatsTask = _matchmakingClient.GetStatsAsync(gameId);
            //
            // try
            // {
            //     await Task.WhenAll(getGameTask, getQueuesTask, getStatsTask);
            // }
            // catch (Exception ex)
            // {
            //     _logger.LogError(ex, "Error loading matchmaking data for game {GameId}", gameId);
            //     ErrorMessage = "Failed to load some matchmaking data. Please try again.";
            // }
            //
            // Game = await getGameTask;
            // Queues = await getQueuesTask;
            // Stats = await getStatsTask;

            Game = await getGameTask;
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading matchmaking queues for game {GameId}", gameId);
            ErrorMessage = "Failed to load matchmaking queues. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm] MatchQueueRequest request)
    {
        try
        {
            await _matchmakingClient.CreateQueueAsync(request with { GameId = GameId });
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating matchmaking queue for game {GameId}", GameId);
            ErrorMessage = "Failed to create queue. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm] MatchQueueRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            // await _matchmakingClient.UpdateQueueAsync(request.Id, request with { GameId = GameId });
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error updating queue {QueueId} for game {GameId}", request.Id, GameId);
            ErrorMessage = "Failed to update queue. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] Guid queueId)
    {
        try
        {
            await _matchmakingClient.DeleteQueueAsync(queueId);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting queue {QueueId} for game {GameId}", queueId, GameId);
            ErrorMessage = "Failed to delete queue. Please try again.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostToggleAsync([FromForm] Guid queueId, [FromForm] bool isEnabled)
    {
        try
        {
            var queue = await _matchmakingClient.GetQueueAsync(queueId);
            // await _matchmakingClient.UpdateQueueAsync(queueId, queue with { IsEnabled = isEnabled });
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling queue {QueueId} for game {GameId}", queueId, GameId);
            ErrorMessage = "Failed to toggle queue status. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnGetQueueAsync([FromQuery] Guid queueId)
    {
        var queue = await _matchmakingClient.GetQueueAsync(queueId);
        if (queue == null)
            return NotFound(new { error = "Queue not found" });

        return new JsonResult(queue);
    }

    public async Task<IActionResult> OnGetQueueStatsAsync([FromQuery] Guid queueId)
    {
        // var stats = await _matchmakingClient.GetQueueStatsAsync(queueId);
        // return new JsonResult(stats);

        throw new NotImplementedException();
    }

    public async Task<IActionResult> OnGetSearchAsync([FromQuery] string query)
    {
        try
        {
            // var queues = await _matchmakingClient.GetQueuesAsync(new PageableRequest
            // {
            //     PageIndex = 0,
            //     PageSize = PageSize,
            //     Search = query,
            //     IsAscending = false
            // });

            // return new JsonResult(queues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching queues for game {GameId} with query {Query}", GameId, query);
            return new JsonResult(new { error = "Failed to search queues" })
            {
                StatusCode = 500
            };
        }
        
        throw new NotImplementedException();

    }

    public string GetPageUrl(int pageNumber)
    {
        var url = Url.Page("/game/matchmaking", new
        {
            gameId = GameId,
            page = pageNumber,
            pageSize = PageSize,
            search = Search
        });

        return url;
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (GameId == Guid.Empty)
        {
            context.Result = new BadRequestResult();
            return;
        }

        base.OnPageHandlerExecuting(context);
    }
}