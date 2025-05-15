using Microsoft.EntityFrameworkCore.Storage;
using SalesManagementSystem.Core.Entities;
using SalesManagementSystem.Core.Interfaces.Repositories;
using SalesManagementSystem.Core.Settings;

namespace SalesManagementSystem.Core.Interfaces.IUnitOfWork;

public interface IUnitOfWork : IDisposable, IScopedInterface
{
    public IBaseRepository<Product> ProductRepository { get; }
    public IOrderRepository OrderRepository { get; }
    public IBaseRepository<OrderItem> OrderItemRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}