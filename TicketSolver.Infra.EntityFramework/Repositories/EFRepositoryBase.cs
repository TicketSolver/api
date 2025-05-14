using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TicketSolver.Infra.EntityFramework.Persistence;
using TicketSolver.Infra.EntityFramework.Repositories.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories;

public abstract class EFRepositoryBase<TEntity> : IEFRepositoryBase<TEntity> where TEntity : class
{
    protected readonly EFContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected EFRepositoryBase(EFContext context)
    {
        Context = context;
        DbSet = Context.Set<TEntity>();
    }

    public virtual IQueryable<TEntity> GetAll()
    {
        return DbSet.AsQueryable();
    }

    private IQueryable<TEntity> GetQueryableById(int id)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Property(parameter, "Id");
        var equals = Expression.Equal(property, Expression.Constant(id));
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        return DbSet.Where(lambda).AsQueryable();
    }

    public IQueryable<TEntity> GetById(int id)
    {
        return GetQueryableById(id);
    }

    public Task<TEntity?> GetById(CancellationToken cancellationToken, int id)
    {
        var query = GetQueryableById(id);

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FindAsync(CancellationToken cancellationToken, int id)
    {
        return await DbSet.FindAsync(id, cancellationToken);
    }

    public virtual async Task<IList<TEntity>> GetListAsync(CancellationToken cancellationToken)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual string GetTenant()
    {
        return Context.Database.GetDbConnection().Database;
    }

    public virtual async Task<bool> InsertAsync(CancellationToken cancellationToken, TEntity entity)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        DbSet.Entry(entity).State = EntityState.Added;

        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public virtual async Task<bool> UpdateAsync(CancellationToken cancellationToken, TEntity entity)
    {
        DbSet.Update(entity);
        DbSet.Entry(entity).State = EntityState.Modified;

        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public virtual async Task<int> DirectUpdateAsync(CancellationToken cancellationToken,
        IQueryable<TEntity> entity, Dictionary<string, object> dbparams)
    {
        if (dbparams == null || !dbparams.Any())
        {
            throw new ArgumentException("dbparams cannot be null or empty", nameof(dbparams));
        }

        var setter = CreateSetter(dbparams);

        return await entity.ExecuteUpdateAsync(setter, cancellationToken);
    }

    public virtual async Task<int> DirectUpdateAsync(CancellationToken cancellationToken,
        int Id, Dictionary<string, object> dbparams)
    {
        var query = DbSet.Where(e => EF.Property<int>(e, "Id") == Id);

        return await DirectUpdateAsync(cancellationToken, query, dbparams);
    }

    public virtual async Task<int> DirectUpdateAsync(CancellationToken cancellationToken,
        List<int> Ids, Dictionary<string, object> dbparams)
    {
        var query = DbSet.Where(e => Ids.Contains(EF.Property<int>(e, "Id")));

        return await DirectUpdateAsync(cancellationToken, query, dbparams);
    }

    private Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> CreateSetter(
        Dictionary<string, object> dbparams)
    {
        var parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "setter");

        Expression body = Expression.Constant(parameter);

        foreach (var param in dbparams)
        {
            var propertyName = param.Key;
            var propertyValue = param.Value;

            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(TEntity)}'.");
            }

            var propertyType = propertyInfo.PropertyType;
            var nonNullableType = GetNonNullableType(propertyType);

            var propertyLambda = CreatePropertyLambda(propertyName, nonNullableType);

            var setPropertyMethod = GetSetPropertyMethod(nonNullableType);

            var valueExpression = CreateValueExpression(propertyValue, nonNullableType);

            var call = Expression.Call(parameter, setPropertyMethod, propertyLambda, valueExpression);

            body = Expression.Block(body, call);
        }

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body, parameter);
    }

    private MethodInfo? GetSetPropertyMethodd(Type propertyType)
    {
        return typeof(SetPropertyCalls<TEntity>)
            .GetMethods()
            .FirstOrDefault(m => m.Name == nameof(SetPropertyCalls<TEntity>.SetProperty) &&
                                 m.IsGenericMethod &&
                                 m.GetParameters().Length == 2)
            ?.MakeGenericMethod(propertyType);
    }

    private MethodInfo? GetSetPropertyMethod(Type propertyType)
    {
        // Obtenha todos os métodos públicos da instância
        var methods = typeof(SetPropertyCalls<TEntity>)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "SetProperty" && m.IsGenericMethod)
            .ToList();

        foreach (var method in methods)
        {
            var parameters = method.GetParameters();

            // Verifique se o método tem dois parâmetros
            if (parameters.Length == 2)
            {
                // Verifique o tipo do primeiro parâmetro
                var firstParamType = parameters[0].ParameterType;
                var secondParamType = parameters[1].ParameterType;

                // Verifique se o primeiro parâmetro é uma expressão e o segundo é do tipo correto
                if (firstParamType.GetGenericTypeDefinition() == typeof(Expression<>) &&
                    secondParamType == propertyType)
                {
                    // Retorne o método genérico com o tipo correto
                    return method.MakeGenericMethod(propertyType);
                }
            }
        }

        // Nenhum método correspondente encontrado
        return null;
    }

    private MethodInfo? GetSetPropertyMethod3(Type propertyType)
    {
        return typeof(SetPropertyCalls<TEntity>)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "SetProperty" &&
                                 m.IsGenericMethod &&
                                 m.GetParameters().Length == 2 &&
                                 m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() ==
                                 typeof(Expression<>) &&
                                 m.GetParameters()[1].ParameterType == propertyType)
            ?.MakeGenericMethod(propertyType);
    }

    private Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> CreateSetter2(
        Dictionary<string, object> dbparams)
    {
        var parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "setter");

        Expression body = parameter;

        foreach (var param in dbparams)
        {
            var propertyName = param.Key;
            var propertyValue = param.Value;

            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(TEntity)}'.");
            }

            var propertyType = propertyInfo.PropertyType;
            var nonNullableType = GetNonNullableType(propertyType);

            var propertyLambda = CreatePropertyLambda(propertyName, nonNullableType);

            var setPropertyMethod = GetSetPropertyMethodd(nonNullableType);

            // Create the value expression
            var valueExpression = CreateValueExpression(propertyValue, nonNullableType);

            // Create the call expression for the setter
            var call = Expression.Call(parameter, setPropertyMethod, propertyLambda, valueExpression);

            body = Expression.Block(body, call);
        }

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body, parameter);
    }

    Type GetNonNullableType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Nullable.GetUnderlyingType(type);
        }

        return type;
    }

    private LambdaExpression CreatePropertyLambda(string propertyName, Type propertyType)
    {
        var entityParam = Expression.Parameter(typeof(TEntity), "e");
        var propertyExpression = Expression.Property(entityParam, propertyName);

        // Obter tipo não anulável
        var nonNullableType = GetNonNullableType(propertyType);

        // Converter a propriedade para o tipo não anulável se necessário
        Expression convertedPropertyExpression = propertyExpression;
        if (propertyExpression.Type != nonNullableType)
        {
            convertedPropertyExpression = Expression.Convert(propertyExpression, nonNullableType);
        }

        // Criar o tipo de expressão lambda apropriado
        var lambdaType = typeof(Func<,>).MakeGenericType(typeof(TEntity), nonNullableType);
        var lambda = Expression.Lambda(lambdaType, convertedPropertyExpression, entityParam);

        return lambda;
    }

    private LambdaExpression CreatePropertyLambdaa(string propertyName, Type propertyType)
    {
        var entityParam = Expression.Parameter(typeof(TEntity), "e");
        var propertyExpression = Expression.Property(entityParam, propertyName);

        // Create a lambda expression that returns the property value
        var lambda = Expression.Lambda(
            typeof(Func<,>).MakeGenericType(typeof(TEntity), propertyType),
            propertyExpression,
            entityParam
        );

        return lambda;
    }

    private Expression CreateValueExpression(object propertyValue, Type propertyType)
    {
        if (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
            return Expression.Constant(propertyValue, propertyType);

        var underlyingType = Nullable.GetUnderlyingType(propertyType);
        return Expression.Convert(Expression.Constant(propertyValue, underlyingType), propertyType);
    }

    public virtual async Task<bool> UpdateAsync(CancellationToken cancellationToken, List<TEntity> entityList)
    {
        foreach (var entity in entityList)
        {
            DbSet.Update(entity);
            DbSet.Entry(entity).State = EntityState.Modified;
        }

        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, TEntity entity)
    {
        DbSet.Remove(entity);
        DbSet.Entry(entity).State = EntityState.Deleted;

        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public virtual async Task<bool> UpdateRangeAsync(CancellationToken cancellationToken, List<TEntity> entity)
    {
        DbSet.UpdateRange(entity);

        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public virtual async Task<bool> AddRangeAsync(CancellationToken cancellationToken, List<TEntity> entity)
    {
        await DbSet.AddRangeAsync(entity, cancellationToken);

        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public virtual async Task<bool> RemoveRangeAsync(CancellationToken cancellationToken, List<TEntity> entity)
    {
        DbSet.RemoveRange(entity);

        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }
}