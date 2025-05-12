using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TicketSolver.Api.Settings;
using TicketSolver.Application.Configuration;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Api.Main;

public static class ConfigureIdentity
{
    public static void Setup(IServiceCollection services)
    {
        services.AddIdentity<Users, IdentityRole>()
            .AddEntityFrameworkStores<EFContext>()
            .AddDefaultTokenProviders(); ;

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
        var jwtExpiration = Environment.GetEnvironmentVariable("JWT_EXPIRATION")!;
        var secret = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddSingleton<IJwtSettings, JwtSettings>(_ => new JwtSettings
        {
            Expiration = int.Parse(jwtExpiration),
            JwtKey = jwtSecret,
        });
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
    }
}