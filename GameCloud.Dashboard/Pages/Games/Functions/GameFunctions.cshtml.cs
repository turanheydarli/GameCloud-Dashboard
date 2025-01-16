using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games.Functions;

public class GameFunctionsModel(IGameClient gameClient, ILogger<GameFunctionsModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid GameId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 0;

    public GameResponse Game { get; private set; }
    public PageableListResponse<FunctionResponse> Functions { get; private set; }
    public string? ErrorMessage { get; private set; }

    public ActionListStatsResponse Stats { get; private set; }

    public async Task<IActionResult> OnGetAsync([FromRoute] Guid gameId)
    {
        try
        {
            var getGameTask = gameClient.GetAsync(gameId);
            var getFunctionsTask = gameClient.GetFunctionsAsync(gameId, new PageableRequest
            {
                PageIndex = Page,
                PageSize = PageSize,
            });
            var getStatsTask = gameClient.GetListFunctionStatsAsync(gameId);

            try
            {
                // Await all tasks concurrently
                await Task.WhenAll(getGameTask, getFunctionsTask, getStatsTask);
            }
            catch (Exception ex)
            {
                // Log error and set error message for partial failures
                logger.LogError(ex, "Error loading game data for game {GameId}", gameId);
                ErrorMessage = "Failed to load some game data. Please try again.";
            }

            // Assign results to properties
            Game = await getGameTask;
            Functions = await getFunctionsTask;
            Stats = await getStatsTask;

            return Page();
        }
        catch (Exception ex)
        {
            // Log error and set error message for complete failure
            logger.LogError(ex, "Error loading game functions for game {GameId}", gameId);
            ErrorMessage = "Failed to load game functions. Please try again.";
            return Page();
        }
    }


    public async Task<IActionResult> OnPostCreateAsync([FromForm] FunctionRequest request)
    {
        await gameClient.CreateFunctionAsync(GameId, request with
        {
            GameId = GameId,
            IsEnabled = true
        });

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm] FunctionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await gameClient.UpdateFunctionAsync(GameId, request.Id, request with
            {
                GameId = GameId
            });

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating function {FunctionId} for game {GameId}",
                request.Id, GameId);
            ErrorMessage = "Failed to update function. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] Guid functionId)
    {
        try
        {
            await gameClient.DeleteFunctionAsync(GameId, functionId);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting function {FunctionId} for game {GameId}",
                functionId, GameId);
            ErrorMessage = "Failed to delete function. Please try again.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostToggleAsync([FromForm] Guid functionId, [FromForm] bool isEnabled)
    {
        try
        {
             await gameClient.ToggleFunctionAsync(GameId, functionId, isEnabled);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling function {FunctionId} for game {GameId}",
                functionId, GameId);
            ErrorMessage = "Failed to toggle function status. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnGetFunctionAsync([FromQuery] Guid functionId)
    {
        var function = await gameClient.GetFunctionAsync(GameId, functionId);
        if (function == null)
            return NotFound(new { error = "Function not found" });

        return new JsonResult(function);
    }

    public string GetPageUrl(int pageNumber)
    {
        var url = Url.Page($"/game/functions", new
        {
            gameId = GameId,
            page = pageNumber,
            pageSize = PageSize,
            search = Search
        });

        return url;
    }

    public async Task<IActionResult> OnGetSearchAsync([FromQuery] string query)
    {
        try
        {
            var functions = await gameClient.GetFunctionsAsync(GameId, new PageableRequest
            {
                PageIndex = 0,
                PageSize = PageSize,
            });

            return new JsonResult(functions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching functions for game {GameId} with query {Query}",
                GameId, query);
            return new JsonResult(new { error = "Failed to search functions" })
            {
                StatusCode = 500
            };
        }
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