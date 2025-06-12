using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Services.Authorization.Requirements;

// Twój DbContext

public class AssignedMechanicHandler : AuthorizationHandler<AssignedMechanicRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssignedMechanicHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AssignedMechanicRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            // Jeśli użytkownik jest administratorem, nie sprawdzaj dalej
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            context.Fail();
            return;
        }

        // Spróbuj wyciągnąć ID zlecenia z route data
        var routeData = _httpContextAccessor.HttpContext?.GetRouteData();
        if (routeData == null)
        {
            context.Fail();
            return;
        }

        if (!routeData.Values.TryGetValue("id", out var idValue))
        {
            context.Fail();
            return;
        }

        if (!int.TryParse(idValue?.ToString(), out var orderId))
        {
            context.Fail();
            return;
        }

        // Znajdź zlecenie w bazie
        var serviceOrder = await _context.ServiceOrders.FindAsync(orderId);
        if (serviceOrder == null)
        {
            context.Fail();
            return;
        }

        // Sprawdź czy current user to przypisany mechanik
        if (serviceOrder.MechanicId == userId || serviceOrder.MechanicId == null)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}