using Microsoft.EntityFrameworkCore;
using SalesManagementSystem.Core.Interfaces.Repositories;
using SalesManagementSystem.EF.DataContext;
using SalesManagementSystem.Shared.Constants;
using SalesManagementSystem.Shared.Pagination;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SalesManagementSystem.EF.Implementation.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected AppDbContext _context;

    private static readonly ConcurrentDictionary<Type, MethodInfo> _setMethodCache = new ConcurrentDictionary<Type, MethodInfo>();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _setPropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _setPropertyDtoCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
    public BaseRepository(AppDbContext context)
    {
        _context = context;
    }

    #region Get Data Functions

    public async Task<bool> Any(Expression<Func<T, bool>> match)
    {
        IQueryable<T> query = _context.Set<T>();
        return await query.AnyAsync(match);
    }

    public async Task<T> FindAsync(Expression<Func<T, bool>> match, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        var result = await query.SingleOrDefaultAsync(match);
        return result;

    }

    public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? take, int? skip,
    Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending)
    {
        IQueryable<T> query = _context.Set<T>().Where(criteria);

        if (take.HasValue)
            query = query.Take(take.Value);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (orderBy != null)
        {
            if (orderByDirection == OrderBy.Ascending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);
        }

        return await query.ToListAsync();
    }

    public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> criteria,
    Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending)
    {
        IQueryable<T> query = _context.Set<T>().Where(criteria);

        if (orderBy != null)
        {
            if (orderByDirection == OrderBy.Ascending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);
        }

        return await query.ToListAsync();
    }

    public async Task<TResult?> FindAndGetSingleDataAsync<TResult>(Expression<Func<T, bool>> match, Expression<Func<T, TResult>> selector, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include).AsSingleQuery();
            }
        }

        return await query
            .Where(match)
            .Select(selector)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TResult>> FindAndGetMultiDataAsync<TResult>(
        Expression<Func<T, bool>> match, Expression<Func<T, TResult>> selector, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include).AsSingleQuery();
            }
        }

        return await query
            .Where(match)
            .Select(selector)
            .ToListAsync();
    }


    public async Task<List<T>> GetAll()
    {
        var Allrecords = await _context.Set<T>().ToListAsync();
        return Allrecords;
    }
    public async Task<List<Tdto>> GetAllWithPojection<Tdto>(Expression<Func<T, Tdto>> selector, params Expression<Func<T, object>>[] includes) where Tdto : class
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        var Allrecords = await query.Select(selector).ToListAsync();
        return Allrecords;
    }
    public async Task<int> GetNumberOfEntityByCondition(Expression<Func<T, bool>> match)
    {
        return await _context.Set<T>().CountAsync(match);
    }

    public async Task<T> GetByIdAsync(object id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    public async Task<int> GetNumberOfEntity(T entity)
    {
        return await _context.Set<T>().CountAsync();
    }

    //public async Task<List<GenericEntityCount>> GetNumberOfMultiEntities(List<object> entities)
    //{
    //    var allCount = new List<GenericEntityCount>();

    //    foreach (var entity in entities)
    //    {

    //        var entityType = entity.GetType();



    //        if (!_setMethodCache.TryGetValue(entityType, out var dbSetMethod))
    //        {
    //            dbSetMethod = typeof(CodPassDbContext)
    //                .GetMethod("Set", new Type[] { })
    //                .MakeGenericMethod(entityType);
    //            _setMethodCache[entityType] = dbSetMethod;
    //        }

    //        var dbSet = dbSetMethod.Invoke(_context, null);


    //        var queryableSet = dbSet as IQueryable<object>;

    //        var count = await queryableSet.CountAsync();
    //        allCount.Add(new GenericEntityCount { NumberOfEntities = count, EntityName = entityType.Name.ToString() });
    //    }

    //    return allCount;
    //}






    public async Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize,
      Expression<Func<T, object>> orderBy = null,
string orderByDirection = OrderBy.Ascending)
    {
        IQueryable<T> query = _context.Set<T>().Where(filter).AsNoTracking();

        if (orderBy != null)
        {
            query = orderByDirection == OrderBy.Ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);
        }

        var totalRecords = await query.CountAsync();
        var data = await query.Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PagedResult<T>
        {
            Data = data,
            TotalRecords = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<TResult>> GetPagedDataWithSelectionAsync<TResult>(int pageNumber, int pageSize,
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
    string orderByDirection = OrderBy.Ascending) where TResult : class
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (orderBy != null)
        {
            query = orderByDirection == OrderBy.Ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);
        }

        var totalRecords = await query.CountAsync();
        var data = await query.Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize).Select(selector)
                   .ToListAsync();

        return new PagedResult<TResult>
        {
            Data = data,
            TotalRecords = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }



    #endregion




    #region create Data Functions
    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
        return;
    }
    public async Task<bool> AddByConditionAsync<Entity>(Expression<Func<Entity, bool>> expression, T entity) where Entity : class
    {
        var IsTrue = await _context.Set<Entity>().Where(expression).FirstOrDefaultAsync();
        if (IsTrue == null)
        {
            return false;
        }
        await _context.Set<T>().AddAsync(entity);
        return true;
    }
    #endregion


    #region Update Data Functions
    public async Task<bool> UpdateEntityAsync<TDto>(object id, TDto dto) where TDto : class
    {

        var entity = await _context.Set<T>().FindAsync(id);

        if (entity == null)
        {
            return false;
        }
        var entityType = entity.GetType();

        var dtoType = dto.GetType();

        var entityProperties = new PropertyInfo[] { };

        var dtoProperties = new PropertyInfo[] { };

        if (!_setPropertyCache.TryGetValue(entityType, out var foundproperties))
        {
            entityProperties = typeof(T).GetProperties();

            _setPropertyCache[entityType] = entityProperties;
        }
        else
        {
            entityProperties = foundproperties;
        }


        if (!_setPropertyDtoCache.TryGetValue(dtoType, out var founddtoproperties))
        {
            dtoProperties = typeof(TDto).GetProperties();

            _setPropertyDtoCache[dtoType] = dtoProperties;
        }
        else
        {
            dtoProperties = founddtoproperties;
        }




        foreach (var dtoProp in dtoProperties)
        {

            var entityProp = entityProperties.FirstOrDefault(p => p.Name == dtoProp.Name && p.PropertyType == dtoProp.PropertyType);

            if (entityProp != null && entityProp.Name != "Id" && entityProp.CanRead && entityProp.CanWrite)
            {
                var value = dtoProp.GetValue(dto);
                entityProp.SetValue(entity, value);
            }
        }


        return true;
    }

    #endregion


    #region Delete Data Functions
    public async Task<bool> DeleteByIdAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;

        _context.Set<T>().Remove(entity);
        return true;
    }
    public async Task<bool> DeleteByConditionAsync<Entity>(Expression<Func<Entity, bool>> expression, object entityToDeleteId) where Entity : class
    {
        var IsTrue = await _context.Set<Entity>().Where(expression).FirstOrDefaultAsync();
        if (IsTrue == null)
        {
            return false;
        }
        return await DeleteByIdAsync(entityToDeleteId);

    }
    public bool Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        return true;
    }
    public bool DeleteRange(IEnumerable<T> entity)
    {
        _context.Set<T>().RemoveRange(entity);
        return true;
    }
    #endregion


    #region Other Useful Functions
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await action();

            //use Uint Of work  to save here when open the transcation
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> IsExistAsync(Expression<Func<T, bool>> match)
    {
        IQueryable<T> query = _context.Set<T>();

        return await query
            .Where(match)
            .AnyAsync();
    }
    #endregion



}
