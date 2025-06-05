using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WarsztatSamochodowyApp.Components;

public class AdminPanelButtonViewComponent : ViewComponent
{
    private readonly IAuthorizationService _authorizationService;

    public AdminPanelButtonViewComponent(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var isAuthorized = await _authorizationService.AuthorizeAsync(HttpContext.User, "AdminOnly");

        if (!isAuthorized.Succeeded)
            return Content(""); // nic nie zwraca, jak nie admin

        return View();
    }
}