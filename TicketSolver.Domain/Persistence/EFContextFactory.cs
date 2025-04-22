using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TicketSolver.Domain.Settings;

namespace TicketSolver.Domain.Persistence
{
    public class EFContextFactory : IDesignTimeDbContextFactory<EFContext>
    {
        public EFContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<EFContext>();
            var sqlConnectionString = DataBaseConnection.DbConnection;
            
            switch (DataBaseConnection.Provider)
            {
                case "MSSQL":

                    options.UseSqlServer(sqlConnectionString,
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 2,
                                maxRetryDelay: TimeSpan.FromSeconds(2),
                                errorNumbersToAdd: null);
                        });
                    break;
        
                case "MYSQL":

                        if (sqlConnectionString != null) options.UseMySQL(sqlConnectionString);
                    break;
        
                case "POSTGRESS":
                        options.UseNpgsql(sqlConnectionString,
                            npgsqlOptionsAction: sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 2,
                                    maxRetryDelay: TimeSpan.FromSeconds(2),
                                    errorCodesToAdd: null);
                            });
                    break;
            }
            return new EFContext(options.Options);
        }
    }
}