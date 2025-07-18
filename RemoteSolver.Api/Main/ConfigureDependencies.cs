namespace RemoteSolver.Api.Main;

public static class ConfigureDependencies
{
    public static void Setup(IServiceCollection services)
    {
        services.AddControllers();
        services.AddMvcCore().AddApiExplorer();
    }
}
