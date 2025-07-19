using Microsoft.EntityFrameworkCore;
using MobileSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Domain.Settings;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace MobileSolver.Api.Main;

public static class ConfigureDbConnection
{
    public static void Setup(IServiceCollection services, IConfiguration configuration)
    {
        var sqlConnectionString = DataBaseConnection.DbConnection;
        
        switch (DataBaseConnection.Provider)
        {
            case "MSSQL":
                services.AddDbContext<IEfContext, AppDbContext>(options =>
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
                services.AddDbContext<IEfContext, AppDbContext>(options =>
                {
                    if (sqlConnectionString != null) options.UseMySQL(sqlConnectionString);
                });
                break;
        
            case "POSTGRESS":
                services.AddDbContext<IEfContext, AppDbContext>(options =>
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