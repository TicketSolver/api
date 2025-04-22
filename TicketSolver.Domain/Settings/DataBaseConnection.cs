using DotNetEnv;

namespace TicketSolver.Domain.Settings;

public class DataBaseConnection
{
    private static string? _dbConnection;
    
    private static string? _provider;
    
    public static string? Provider
    {
        get
        {
            if (_provider == null)
            {
                GenerateDbConnection();
            }
            return _provider;
        }
    }

    public static string? DbConnection
    {
        get
        {
            if (_dbConnection == null)
            {
                GenerateDbConnection();
            }
            return _dbConnection;
        }
    }

    private static void GenerateDbConnection()
    {
        Env.Load(".env.db");
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        _dbConnection = $"Host={host};Database={database};Username={user};Password={password};Port={port}";
        _provider = Environment.GetEnvironmentVariable("DB_PROVIDER");
    }
}