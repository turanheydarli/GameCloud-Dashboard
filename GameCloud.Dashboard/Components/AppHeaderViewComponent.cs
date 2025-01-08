using GameCloud.Dashboard.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.Dashboard.Components;

public class AppHeaderViewComponent(IDeveloperClient developerClient) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var developer = await developerClient.GetMeAsync();
        
        return View(developer);
    }
}