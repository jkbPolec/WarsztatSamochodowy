using Microsoft.AspNetCore.Authorization;
using WarsztatSamochodowyApp.Services.Authorization.Requirements;

namespace WarsztatSamochodowyApp.Services.Authorization;

public static class AuthorizationPolicies
{
    public static void AddCustomAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy("AdminOnly", policy => { policy.RequireRole("Admin"); });
        options.AddPolicy("CarPartsPolicy", policy => { policy.RequireRole("Admin", "Mechanik"); });
        options.AddPolicy("CarRegistrationPolicy", policy => { policy.RequireRole("Admin", "Recepcjonista"); });
        options.AddPolicy("ServiceOrderPolicy",
            policy => { policy.RequireRole("Admin", "Recepcjonista", "Mechanik"); });
        options.AddPolicy("ServiceTaskPolicy", policy => { policy.RequireRole("Admin", "Mechanik"); });
        options.AddPolicy("OnlyAssignedMechanic",
            policy => { policy.Requirements.Add(new AssignedMechanicRequirement()); });
    }
}