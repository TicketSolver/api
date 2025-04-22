using DotNetEnv;
using Microsoft.Extensions.Configuration;

namespace TicketSolver.Domain.Extensions;

public static class BuilderEnvExtensions
{
    public static IConfigurationBuilder AddEnvironmentBasedDotEnv(this IConfigurationBuilder builder)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var envFile = $".env.{env.ToLower()}";

        if (File.Exists(".env"))
            Env.Load(".env");
        
        if (File.Exists(envFile))
            Env.Load(envFile);
        
        return builder;
    }
}