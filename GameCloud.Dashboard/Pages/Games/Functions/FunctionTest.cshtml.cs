using System.Text.Json;
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

    [BindProperty] public ActionRequest TestRequest { get; set; }
    public GameResponse Game { get; set; }
    public FunctionResponse Function { get; set; }
    public PageableListResponse<ActionResponse> RecentTests { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Game = await gameClient.GetAsync(GameId);

        Function = await gameClient.GetFunctionAsync(GameId, FunctionId);

        RecentTests = await gameClient.GetFunctionLogsAsync(GameId, FunctionId, new DynamicRequest(
            Sort: new Sort[]
            {
                new Sort("CreatedAt", "desc")
            },
            Filter: new Filter("IsTestMode", "eq", "true", null, null),
            PageIndex: 0,
            PageSize: 10
        ));

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromBody] ActionRequest request)
    {
        var defaultGameKey = await gameClient.GetOrCreateDefaultGameKey(GameId);
        var testResult = await gameClient.TestFunctionAsync(GameId, FunctionId, request,
            defaultGameKey.ApiKey);
        
        return new JsonResult(testResult);
    }
}