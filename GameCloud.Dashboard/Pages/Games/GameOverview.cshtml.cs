using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games;

public class GameOverviewModel(IGameClient gameClient) : PageModel
{
    public GameDetailResponse Game { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid gameId)
    {
        Game = await gameClient.GetGameDetailsAsync(gameId);
        return Page();
    }
}