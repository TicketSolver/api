using TicketSolver.Infra.Storage.Extensions;

namespace TicketSolver.Api.Main;

public static class ConfigureDependencies
{
    public static void Setup(IServiceCollection services)
    {
        services.AddControllers();
        services.AddMvcCore().AddApiExplorer();
    }
}