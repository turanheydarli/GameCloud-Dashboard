using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games.Functions;

public class FunctionTestModel(IGameClient gameClient) : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid GameId { get; set; }

    [BindProperty(SupportsGet = true)] public Guid FunctionId { get; set; }
    public GameResponse Game { get; set; }
    public FunctionResponse Function { get; set; }
    public PageableListResponse<FunctionResponse> RecentTests { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Game = await gameClient.GetAsync(GameId);

        Function = await gameClient.GetFunctionAsync(GameId, FunctionId);

        RecentTests = await gameClient.GetFunctionsAsync(GameId, new PageableRequest());

        return Page();
    }
}