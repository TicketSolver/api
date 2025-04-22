using Microsoft.AspNetCore.Identity;
using TicketSolver.Domain.Persistence;

namespace TicketSolver.Api.Main;

public static class ConfigureIdentity
{
    public static void Setup(IServiceCollection services)
    {
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit           = false;
                options.Password.RequireLowercase       = false;
                options.Password.RequireUppercase       = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength         = 6;
            })
            .AddEntityFrameworkStores<EFContext>()
            .AddDefaultTokenProviders();
    }
}