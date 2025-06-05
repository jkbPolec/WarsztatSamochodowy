using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WarsztatSamochodowyApp.Components;

[Authorize(Policy = "OnlyAdmins")]
public class AdminPanelButtonViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}