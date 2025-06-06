using Microsoft.AspNetCore.Authorization;

namespace WarsztatSamochodowyApp.Services.Authorization;

public static class AuthorizationPolicies
{
    public static void AddCustomAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy("AdminOnly", policy => { policy.RequireRole("Admin"); });
        options.AddPolicy("CarPartsPolicy", policy => { policy.RequireRole("Admin", "Mechanik"); });
    }
}