using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TicketSolver.Api.Settings.Swagger;

public static class SwaggerExtensions
{
    private static IServiceProvider _serviceProvider;
    public static void AddSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            AddSwaggerDoc(options, "v1");

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlDocumentPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

            if (File.Exists(xmlDocumentPath))
            {
                options.IncludeXmlComments(xmlDocumentPath);
            }
        });
    }

    private static void AddSwaggerDoc(SwaggerGenOptions options, string version)
    {
        options.SwaggerDoc(version, new OpenApiInfo
        {
            Version = version,
            Title = "TicketSolver",
            Description = "Ticket Solver Api",
            TermsOfService = new Uri("http://www.github.com/TicketSovler"),
            License = new OpenApiLicense
            {
                Name = "License",
                Url = new Uri("https://www.github.com/TicketSolver")
            }
        });
    }

    public static void UseSwagger(WebApplication app, WebApplicationBuilder builder)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}