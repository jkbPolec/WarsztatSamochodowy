using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WarsztatSamochodowyApp.Components;

public class HeaderViewComponent : ViewComponent
{
    private readonly IAuthorizationService _authorizationService;

    public HeaderViewComponent(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string policyName, string viewName)
    {
        var result = await _authorizationService.AuthorizeAsync(HttpContext.User, policyName);


        if (!result.Succeeded)
            return Content(""); // Brak autoryzacji = brak przycisków

        return View(viewName);
    }
}