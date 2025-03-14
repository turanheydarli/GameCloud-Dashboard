using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games.Matchmaking;

public class MatchmakingQueuesModel : PageModel
{
    private readonly IMatchmakingClient _matchmakingClient;
    private readonly IGameClient _gameClient;
    private readonly ILogger<MatchmakingQueuesModel> _logger;

    public MatchmakingQueuesModel(
        IMatchmakingClient matchmakingClient,
        IGameClient gameClient,
        ILogger<MatchmakingQueuesModel> logger)
    {
        _matchmakingClient = matchmakingClient;
        _gameClient = gameClient;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)] public Guid GameId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)] public int PageIndex { get; set; } = 0;
    [BindProperty(SupportsGet = true)] public GameKeyResponse DefaultGameKey { get; set; }

    public GameResponse Game { get; set; } = default!;
    public PageableListResponse<MatchmakingResponse> Queues { get; set; } = default!;
    public MatchmakingStatsResponse Stats { get; set; } = default!;
    public IEnumerable<FunctionResponse> AvailableFunctions { get; set; } = new List<FunctionResponse>();

    public async Task<IActionResult> OnGetAsync()
    {
        DefaultGameKey = await _gameClient.GetOrCreateDefaultGameKey(GameId);

        Game = await _gameClient.GetAsync(GameId);


        Queues = await _matchmakingClient.GetQueuesAsync(DefaultGameKey.ApiKey, Search, PageIndex, PageSize);

        Stats = await _matchmakingClient.GetStatsAsync(DefaultGameKey.ApiKey);

        var functions = await _gameClient.GetFunctionsAsync(GameId, new PageableRequest
        {
            PageIndex = 0,
            PageSize = int.MaxValue,
            Search = null,
            IsAscending = false
        });

        AvailableFunctions = functions.Items.Where(f => f.IsEnabled);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromBody] MatchQueueRequest request)
    {
        var result = await _matchmakingClient.CreateQueueAsync(DefaultGameKey.ApiKey, request);
        return RedirectToPage("/Game/{gameId}/Matchmaking", new { gameId = GameId });
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromBody] MatchQueueRequest request)
    {
        try
        {
            var result = await _matchmakingClient.UpdateQueueAsync(DefaultGameKey.ApiKey, request.Id, request);
            return RedirectToPage("/Game/{gameId}/Matchmaking", new { gameId = GameId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating matchmaking queue {QueueId}", request.Id);
            ModelState.AddModelError(string.Empty, "Failed to update queue: " + ex.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            await _matchmakingClient.DeleteQueueAsync(DefaultGameKey.ApiKey, id);
            return RedirectToPage("/Game/{gameId}/Matchmaking", new { gameId = GameId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting matchmaking queue {QueueId}", id);
            ModelState.AddModelError(string.Empty, "Failed to delete queue: " + ex.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid id, bool isEnabled)
    {
        try
        {
            var request = new ToggleQueueRequest(isEnabled);

            var result = await _matchmakingClient.ToggleQueueAsync(DefaultGameKey.ApiKey, id, request);

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return new JsonResult(result);
            }

            return RedirectToPage("/Game/{gameId}/Matchmaking", new { gameId = GameId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling matchmaking queue {QueueId}", id);

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return BadRequest(new { error = ex.Message });
            }

            ModelState.AddModelError(string.Empty, "Failed to toggle queue: " + ex.Message);
            return Page();
        }
    }

    public string GetQueueTypeName(string queueType) =>
        queueType switch
        {
            "TurnBased" => "Turn-Based",
            "Simultaneous" => "Simultaneous",
            "Asynchronous" => "Asynchronous",
            _ => queueType
        };

    public string GetQueueTypeColor(string queueType) =>
        queueType switch
        {
            "TurnBased" => "primary",
            "Simultaneous" => "success",
            "Asynchronous" => "info",
            _ => "secondary"
        };

    public string GetQueueTypeIcon(string queueType) =>
        queueType switch
        {
            "TurnBased" => "ri-clockwise-line",
            "Simultaneous" => "ri-group-line",
            "Asynchronous" => "ri-time-line",
            _ => "ri-gamepad-line"
        };

    public string FormatTimeSpan(TimeSpan? timeSpan)
    {
        if (!timeSpan.HasValue || timeSpan.Value.TotalSeconds == 0)
            return "None";

        var totalSeconds = timeSpan.Value.TotalSeconds;

        if (totalSeconds < 60) return $"{totalSeconds:0}s";
        if (totalSeconds < 3600) return $"{Math.Floor(totalSeconds / 60)}m";
        if (totalSeconds < 86400) return $"{Math.Floor(totalSeconds / 3600)}h";
        return $"{Math.Floor(totalSeconds / 86400)}d";
    }

    public string? GetPageUrl(int pageIndex)
    {
        return Url.Page("/game/{gameId}/matchmaking", new
        {
            gameId = GameId,
            pageIndex,
            pageSize = PageSize,
            search = Search
        });
    }
}