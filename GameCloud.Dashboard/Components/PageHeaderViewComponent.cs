using GameCloud.Dashboard.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.Dashboard.Components;

public class PageHeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ViewConfiguration config)
    {
        return View(config);
    }
}