using DotNetEnv;

namespace TicketSolver.Domain.Settings;

public class DataBaseConnection
{
    private static string? _dbConnection;
    private static string? _provider;

    public static string? DbConnection
    {
        get
        {
            if (_dbConnection == null)
            {
                ReadEnvDbConnection();
            }
            return _dbConnection;
        }
    }
    
    public static string? Provider
    {
        get
        {
            if (_provider == null)
            {
                ReadEnvDbConnection();
            }
            return _provider;
        }
    }

    private static void ReadEnvDbConnection()
    {
        Env.Load(".env.db");
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        
        _provider = Environment.GetEnvironmentVariable("DB_PROVIDER");
        _dbConnection = $"Host={host};Database={database};Username={user};Password={password};Port={port}";
    }
}