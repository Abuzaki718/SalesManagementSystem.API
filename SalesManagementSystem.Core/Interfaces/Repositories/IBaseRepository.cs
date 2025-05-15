using SalesManagementSystem.Shared.Constants;
using SalesManagementSystem.Shared.Pagination;
using System.Linq.Expressions;

namespace SalesManagementSystem.Core.Interfaces.Repositories;


public interface IBaseRepository<T> where T : class
{
    Task<List<T>> GetAll();
    Task<List<Tdto>> GetAllWithPojection<Tdto>(Expression<Func<T, Tdto>> selector, params Expression<Func<T, object>>[] includes) where Tdto : class;
    Task<T> FindAsync(Expression<Func<T, bool>> match, params Expression<Func<T, object>>[] includes);
    Task<bool> Any(Expression<Func<T, bool>> match);
    Task<List<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? skip, int? take,
       Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending);

    Task<List<T>> FindAllAsync(Expression<Func<T, bool>> criteria,
    Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending);

    Task<TResult?> FindAndGetSingleDataAsync<TResult>(Expression<Func<T, bool>> match, Expression<Func<T, TResult>> selector, params Expression<Func<T, object>>[] includes);

    Task<List<TResult>> FindAndGetMultiDataAsync<TResult>(
         Expression<Func<T, bool>> match, Expression<Func<T, TResult>> selector, params Expression<Func<T, object>>[] includes);

    Task<T> GetByIdAsync(object id);
    Task<int> GetNumberOfEntity(T entity);

    Task<int> GetNumberOfEntityByCondition(Expression<Func<T, bool>> match);

    //Task<List<GenericEntityCount>> GetNumberOfMultiEntities(List<object> entities);
    Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize,
      Expression<Func<T, object>> orderBy = null,
string orderByDirection = OrderBy.Ascending);

    Task<PagedResult<TResult>> GetPagedDataWithSelectionAsync<TResult>(int pageNumber, int pageSize,
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
    string orderByDirection = OrderBy.Ascending) where TResult : class;


    Task<bool> UpdateEntityAsync<TDto>(object id, TDto dto) where TDto : class;

    Task<bool> AddByConditionAsync<Entity>(Expression<Func<Entity, bool>> expression, T entity) where Entity : class;
    Task<T> AddAsync(T entity);

    Task AddRangeAsync(IEnumerable<T> entities);
    Task<bool> DeleteByConditionAsync<Entity>(Expression<Func<Entity, bool>> expression, object entityToDeleteId) where Entity : class;
    Task<bool> DeleteByIdAsync(object id);

    bool Delete(T entity);
    bool DeleteRange(IEnumerable<T> entity);



    Task ExecuteInTransactionAsync(Func<Task> action);

    Task<bool> IsExistAsync(Expression<Func<T, bool>> match);


}
