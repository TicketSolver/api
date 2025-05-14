using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginatedQuery query)
    {
        return queryable
            .Skip(query.Skip)
            .Take(query.PageSize);
    }
}