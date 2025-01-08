using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace GameCloud.Dashboard.Pages.Games;

public class CreateGameModel(IGameClient gameClient) : PageModel
{
    [BindProperty] public GameRequest Game { get; set; }

    public void OnGet()
    {
    }

    public async Task OnPostAsync()
    {
        await gameClient.CreateAsync(Game);

        RedirectToPage();
    }
}