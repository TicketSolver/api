using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TicketSolver.Api.Settings;
using TicketSolver.Application.Configuration;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Infra.EntityFramework.Persistence;
using JwtSettings = TicketSolver.Application.Configuration.JwtSettings;

namespace TicketSolver.Api.Main;

public static class ConfigureIdentity
{
    public static void Setup(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.MaxDepth = 64;
            });
        services.AddIdentity<Users, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<EfContext>()
            .AddDefaultTokenProviders();

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
        var jwtExpiration = Environment.GetEnvironmentVariable("JWT_EXPIRATION")!;
        var secret = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddSingleton(new JwtSettings
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