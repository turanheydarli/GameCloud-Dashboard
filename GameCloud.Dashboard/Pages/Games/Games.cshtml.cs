using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace GameCloud.Dashboard.Pages.Games;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Developer")]
public class GamesModel(
    IGameClient gameClient,
    ILogger<GamesModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty] public GameRequest GameRequest { get; set; }

    public PageableListResponse<GameResponse> Games { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Games = await gameClient.GetAllAsync(new PageableRequest
        {
            PageIndex = 0,
            PageSize = 12,
            Search = Search,
            IsAscending = false
        });

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(IFormFile image = null)
    {
        try
        {
            var game = await gameClient.CreateAsync(GameRequest);

            if (image != null)
            {
                try
                {
                    var streamPart = new StreamPart(image.OpenReadStream(), image.FileName, image.ContentType);
                    await gameClient.SetImageAsync(game.Id, streamPart);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error uploading game image for game {GameId}", game.Id);
                    TempData["Warning"] = "Game created successfully but failed to upload image.";
                    return RedirectToPage();
                }
            }

            TempData["Success"] = "Game created successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating game");
            TempData["Error"] = "Failed to create game. Please try again.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["Error"] = "Invalid game specified.";
            return RedirectToPage();
        }

        try
        {
            await gameClient.DeleteAsync(id);
            TempData["Success"] = "Game deleted successfully.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting game {GameId}", id);
            TempData["Error"] = "Failed to delete game. Please try again.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetSearchAsync(string query)
    {
        try
        {
            Games = await gameClient.GetAllAsync(new PageableRequest
            {
                PageIndex = 0,
                PageSize = 12
            });

            return Partial("_GamesGrid", Games);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching games");
            return StatusCode(500, "Failed to search games");
        }
    }

    public async Task<IActionResult> OnPostUploadImageAsync(Guid id, IFormFile image)
    {
        if (id == Guid.Empty || image == null)
        {
            return BadRequest("Invalid game ID or image file");
        }

        try
        {
            var streamPart = new StreamPart(image.OpenReadStream(), image.FileName, image.ContentType);
            await gameClient.SetImageAsync(id, streamPart);
            TempData["Success"] = "Game image updated successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading game image for game {GameId}", id);
            TempData["Error"] = "Failed to upload game image. Please try again.";
            return RedirectToPage();
        }
    }
}