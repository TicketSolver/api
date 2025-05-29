using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Settings;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Api.Main;

public static class ConfigureDbConnection
{
    public static void Setup(IServiceCollection services, IConfiguration configuration)
    {
        var sqlConnectionString = DataBaseConnection.DbConnection;
        
        switch (DataBaseConnection.Provider)
        {
            case "MSSQL":
                services.AddDbContext<EfContext>(options =>
                    options.UseSqlServer(sqlConnectionString,
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 2,
                                maxRetryDelay: TimeSpan.FromSeconds(2),
                                errorNumbersToAdd: null);
                        })
                );
                break;
        
            case "MYSQL":
                services.AddDbContext<EfContext>(options =>
                {
                    if (sqlConnectionString != null) options.UseMySQL(sqlConnectionString);
                });
                break;
        
            case "POSTGRESS":
                services.AddDbContext<EfContext>(options =>
                    options.UseNpgsql(sqlConnectionString,
                        npgsqlOptionsAction: sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 2,
                                maxRetryDelay: TimeSpan.FromSeconds(2),
                                errorCodesToAdd: null);
                        })
                );
                break;
        }
    }
}