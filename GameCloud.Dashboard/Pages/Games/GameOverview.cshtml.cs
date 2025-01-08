using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games;

public class GameOverviewModel(IGameClient gameClient) : PageModel
{
    public GameResponse Game { get; set; }
    public PageableListResponse<GameKeyResponse> Keys { get; set; }

    public async Task OnGetAsync([FromRoute] Guid gameId)
    {
        Game = await gameClient.GetAsync(gameId);
        Keys = await gameClient.GetKeysAsync(gameId, new PageableRequest());
    }
}