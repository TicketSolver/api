namespace TicketSolver.Api.Main;

public static class ConfigureDbConnection
{
    public static void Setup(IServiceCollection services, IConfiguration configuration)
    {
        // var sqlConnectionString = configuration.GetConnectionString(
        //     configuration.GetConnectionString("SqlProviderConnection") ?? string.Empty);
        //
        // switch (configuration.GetConnectionString("SqlProvider"))
        // {
        //     case "MSSQL":
        //         services.AddDbContext<EFContext>(options =>
        //             options.UseSqlServer(sqlConnectionString,
        //                 sqlServerOptionsAction: sqlOptions =>
        //                 {
        //                     sqlOptions.EnableRetryOnFailure(
        //                         maxRetryCount: 2,
        //                         maxRetryDelay: TimeSpan.FromSeconds(2),
        //                         errorNumbersToAdd: null);
        //                 })
        //         );
        //         break;
        //
        //     case "MYSQL":
        //         services.AddDbContext<EFContext>(options =>
        //             options.UseMySQL(sqlConnectionString)
        //         );
        //         break;
        //
        //     case "POSTGRESS":
        //         services.AddDbContext<EFContext>(options =>
        //             options.UseNpgsql(sqlConnectionString,
        //                 npgsqlOptionsAction: sqlOptions =>
        //                 {
        //                     sqlOptions.EnableRetryOnFailure(
        //                         maxRetryCount: 2,
        //                         maxRetryDelay: TimeSpan.FromSeconds(2),
        //                         errorCodesToAdd: null);
        //                 })
        //         );
        //         break;
        // }
    }
}