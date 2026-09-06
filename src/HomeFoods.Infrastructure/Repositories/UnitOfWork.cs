using Microsoft.EntityFrameworkCore.Storage;
using HomeFoods.Domain.Repositories;
using HomeFoods.Infrastructure.Data;

namespace HomeFoods.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly HomeFoodsDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        HomeFoodsDbContext context,
        IUserRepository users,
        IProductRepository products,
        ICategoryRepository categories,
        IBrandRepository brands,
        IOrderRepository orders)
    {
        _context = context;
        Users = users;
        Products = products;
        Categories = categories;
        Brands = brands;
        Orders = orders;
    }

    public IUserRepository Users { get; }
    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public IBrandRepository Brands { get; }
    public IOrderRepository Orders { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
