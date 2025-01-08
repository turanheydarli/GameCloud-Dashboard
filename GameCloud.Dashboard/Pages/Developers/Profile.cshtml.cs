using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Models.Requests;
using GameCloud.Dashboard.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace GameCloud.Dashboard.Pages;

public class Profile(IDeveloperClient developerClient) : PageModel
{
    [BindProperty] public DeveloperResponse Developer { get; set; }

    public async Task OnGetAsync()
    {
        var developer = await developerClient.GetMeAsync();

        Developer = developer;
    }

    public async Task OnPostAsync()
    {
        try
        {
            await developerClient.UpdateMeAsync(new DeveloperRequest(
                Id: Developer.Id,
                UserId: Guid.Empty,
                Name: Developer.Name,
                Email: Developer.Email));
        }
        catch (ApiException e)
        {
            throw;
        }
    }
}