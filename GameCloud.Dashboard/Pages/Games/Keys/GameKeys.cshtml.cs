using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Games.Keys;

public class GameKeysModel : PageModel
{
    private readonly IGameClient _gameClient;
    private readonly ILogger<GameKeysModel> _logger;

    [BindProperty(SupportsGet = true)]
    public Guid GameId { get; set; }

    public PageableListResponse<GameKeyResponse> Keys { get; private set; }
    
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 0;

    public const int PageSize = 10;

    public GameKeysModel(IGameClient gameClient, ILogger<GameKeysModel> logger)
    {
        _gameClient = gameClient;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Keys = await _gameClient.GetKeysAsync(GameId, new PageableRequest 
            { 
                PageIndex = PageNumber,
                PageSize = PageSize 
            });
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading game keys");
            // You can add error handling here, such as setting TempData for displaying error messages
            TempData["ErrorMessage"] = "Failed to load game keys. Please try again.";
            return RedirectToPage("/Games/Index");
        }
    }

    public async Task<IActionResult> OnPostCreateKeyAsync()
    {
        try
        {
            await _gameClient.CreateKeyAsync(GameId);
            TempData["SuccessMessage"] = "Game key created successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game key");
            TempData["ErrorMessage"] = "Failed to create game key. Please try again.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteKeyAsync(string key)
    {
        try
        {
            await _gameClient.DeleteKeyAsync(GameId, key);
            TempData["SuccessMessage"] = "Game key deleted successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game key");
            TempData["ErrorMessage"] = "Failed to delete game key. Please try again.";
            return RedirectToPage();
        }
    }
}