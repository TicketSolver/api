namespace TicketSolver.Api.Settings;
using DotNetEnv;
public class DataBaseConnection
{
    private static string? _dbConnection;

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
    }
}