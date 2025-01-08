using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Responses;
using GameCloud.Dashboard.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages;

[Authorize(Roles = "Developer")]
public class IndexModel(
    ILogger<IndexModel> logger,
    IDeveloperClient developerClient,
    JwtTokenHandler handler) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public DeveloperResponse Me { get; private set; }

    public async Task OnGetAsync()
    {
        Me = await developerClient.GetMeAsync();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        handler.Token = String.Empty;

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToRoute("login");
    }
}