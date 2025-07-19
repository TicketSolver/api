using Microsoft.Extensions.DependencyInjection;
using TicketSolver.Domain.Enums;
using TicketSolver.Infra.Notifications.Email;
using TicketSolver.Infra.Notifications.Email.Interfaces;

namespace TicketSolver.Infra.Notifications.Extensions;

public static class ConfigureEmailSender
{
    public static void AddEmailSenderService(this IServiceCollection services, LifeCicle lifeCicle = LifeCicle.Transient)
    {
        // Melhoria: Configurar de acordo com o lifeCicle
        services.AddTransient<IEmailSenderService, EmailSenderService>();
    }
}