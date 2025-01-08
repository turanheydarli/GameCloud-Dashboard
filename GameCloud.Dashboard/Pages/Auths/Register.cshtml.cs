using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Auths;

public class RegisterModel(IDeveloperClient developerClient) : PageModel
{
    [BindProperty] public RegisterDeveloperRequest RegisterRequest { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await developerClient.RegisterDeveloperAsync(RegisterRequest);
        return RedirectToPage("login");
    }
}