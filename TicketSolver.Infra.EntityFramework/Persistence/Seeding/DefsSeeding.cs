using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;

namespace TicketSolver.Infra.EntityFramework.Persistence.Seeding;

public static class DefsSeeding
{
    public static async Task SeedDefsAsync(EfContext context)
    {
        await SeedTicketCategoriesAsync(context);
        await SeedUserSatisfactionAsync(context);
        await SeedStorageProvidersAsync(context);
        await SeedTicketUserRolesAsync(context);
        await SeedTicketStatusAsync(context);
        await SeedTicketPrioritiesAsync(context);
        await SeedUserTypesAsync(context);
        await SeedUserStatusAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedStorageProvidersAsync(EfContext context)
    {
        if (await context.DefStorageProviders.AnyAsync()) return;

        List<DefStorageProviders> defTicketCategories =
        [
            new(eDefStorageProviders.Aws),
            new(eDefStorageProviders.Azure),
        ];

        await context.DefStorageProviders.AddRangeAsync(defTicketCategories);
    }
    
    private static async Task SeedUserSatisfactionAsync(EfContext context)
    {
        if (await context.DefUserSatisfaction.AnyAsync()) return;

        List<DefUserSatisfaction> defUserSatisfactions =
        [
            new(eDefUserSatisfaction.VeryBad),
            new(eDefUserSatisfaction.Bad),
            new(eDefUserSatisfaction.Neutral),
            new(eDefUserSatisfaction.Good),
            new(eDefUserSatisfaction.Excellent),
        ];

        await context.DefUserSatisfaction.AddRangeAsync(defUserSatisfactions);
    }
    
    private static async Task SeedTicketCategoriesAsync(EfContext context)
    {
        if (await context.DefTicketCategories.AnyAsync()) return;

        List<DefTicketCategories> defTicketCategories =
        [
            new(eDefTicketCategories.Hardware),
            new(eDefTicketCategories.Software),
            new(eDefTicketCategories.Network),
            new(eDefTicketCategories.Access),
        ];

        await context.DefTicketCategories.AddRangeAsync(defTicketCategories);
    }

    private static async Task SeedTicketUserRolesAsync(EfContext context)
    {
        if (await context.DefTicketUserRoles.AnyAsync()) return;

        List<DefTicketUserRoles> defTicketUserRoles =
        [
            new(eDefTicketUserRoles.Requester),
            new(eDefTicketUserRoles.Responder),
            new(eDefTicketUserRoles.Observer)
        ];

        await context.DefTicketUserRoles.AddRangeAsync(defTicketUserRoles);
    }

    private static async Task SeedTicketStatusAsync(EfContext context)
    {
        if (await context.DefTicketStatus.AnyAsync()) return;

        List<DefTicketStatus> defTicketStatus =
        [
            new(eDefTicketStatus.New),
            new(eDefTicketStatus.InProgress),
            new(eDefTicketStatus.Resolved),
            new(eDefTicketStatus.Closed),
            new(eDefTicketStatus.Reopened)
        ];

        await context.DefTicketStatus.AddRangeAsync(defTicketStatus);
    }

    private static async Task SeedTicketPrioritiesAsync(EfContext context)
    {
        if (await context.DefTicketPriorities.AnyAsync()) return;

        List<DefTicketPriorities> defTicketPriorities =
        [
            new(eDefTicketPriorities.Low),
            new(eDefTicketPriorities.Medium),
            new(eDefTicketPriorities.High),
            new(eDefTicketPriorities.Urgent),
        ];

        await context.DefTicketPriorities.AddRangeAsync(defTicketPriorities);
    }

    private static async Task SeedUserTypesAsync(EfContext context)
    {
        if (await context.DefUserTypes.AnyAsync()) return;

        List<DefUserTypes> defUserTypes =
        [
            new(eDefUserTypes.Admin),
            new(eDefUserTypes.Technician),
            new(eDefUserTypes.Client),
        ];

        await context.DefUserTypes.AddRangeAsync(defUserTypes);
    }
    
    private static async Task SeedUserStatusAsync(EfContext context)
    {
        if (await context.DefUserTypes.AnyAsync()) return;

        List<DefUserStatus> defUserTypes =
        [
            new(eDefUserStatus.Active),
            new(eDefUserStatus.Inactive),
            new(eDefUserStatus.Busy),
            new(eDefUserStatus.Absent),
        ];

        await context.DefUserStatus.AddRangeAsync(defUserTypes);
    }
}