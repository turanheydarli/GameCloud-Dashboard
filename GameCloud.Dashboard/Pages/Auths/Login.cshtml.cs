using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace GameCloud.Dashboard.Pages.Auths
{
    public class LoginModel(
        IDeveloperClient developerClient,
        JwtTokenHandler tokenHandler,
        IConfiguration configuration)
        : PageModel
    {
        [BindProperty] public LoginDeveloperRequest LoginRequest { get; set; }

        public IActionResult OnGet(string returnUrl = null)
        {
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                return Redirect(returnUrl ?? "/");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await developerClient.LoginDeveloperAsync(LoginRequest);
                tokenHandler.Token = response.Token;

                return Redirect(returnUrl ?? configuration["App:DefaultRedirectPath"] ?? "/");
            }
            catch (ApiException)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}