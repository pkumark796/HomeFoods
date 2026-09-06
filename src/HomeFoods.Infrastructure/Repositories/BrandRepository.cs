using Microsoft.EntityFrameworkCore;
using HomeFoods.Domain.Entities;
using HomeFoods.Domain.Repositories;
using HomeFoods.Infrastructure.Data;

namespace HomeFoods.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly HomeFoodsDbContext _context;

    public BrandRepository(HomeFoodsDbContext context)
    {
        _context = context;
    }

    public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Brands.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Brand>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Brand brand, CancellationToken cancellationToken = default)
    {
        await _context.Brands.AddAsync(brand, cancellationToken);
    }

    public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default)
    {
        _context.Brands.Update(brand);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _context.Brands.Remove(new Brand { Id = id });
        return Task.CompletedTask;
    }
}
