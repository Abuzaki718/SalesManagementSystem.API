using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Murur.Core.Domain.Identity;
using SalesManagementSystem.Core.Entities;
using SalesManagementSystem.Core.Interfaces.IUnitOfWork;
using SalesManagementSystem.Core.Interfaces.Repositories;
using SalesManagementSystem.EF.DataContext;
using SalesManagementSystem.EF.Implementation.Repositories;

namespace SalesManagementSystem.EF.Implementation.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public IBaseRepository<Product> ProductRepository { get; private set; }

    public IOrderRepository OrderRepository { get; private set; }

    public IBaseRepository<OrderItem> OrderItemRepository { get; private set; }
    public UnitOfWork(AppDbContext context, IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<OrderRepository> logger)
    {
        _dbContext = context;

        ProductRepository = new BaseRepository<Product>(_dbContext);
        OrderRepository = new OrderRepository(_dbContext, userManager, logger);
        OrderItemRepository = new BaseRepository<OrderItem>(_dbContext);

    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_dbContext.Database.CurrentTransaction == null)
            return await _dbContext.Database.BeginTransactionAsync();

        return _dbContext.Database.CurrentTransaction;
    }
    public async Task CommitAsync()
    {
        var transaction = _dbContext.Database.CurrentTransaction;
        if (transaction != null)
            await transaction.CommitAsync();
    }
    public async Task RollbackAsync()
    {
        var transaction = _dbContext.Database.CurrentTransaction;
        if (transaction != null)
            await transaction.RollbackAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int rowChange = await _dbContext.SaveChangesAsync(cancellationToken);
        return rowChange;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
