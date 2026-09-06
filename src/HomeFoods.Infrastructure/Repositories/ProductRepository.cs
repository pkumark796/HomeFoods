using Microsoft.EntityFrameworkCore;
using HomeFoods.Domain.Entities;
using HomeFoods.Domain.Repositories;
using HomeFoods.Infrastructure.Data;

namespace HomeFoods.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly HomeFoodsDbContext _context;

    public ProductRepository(HomeFoodsDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.BrandId == brandId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsFeatured && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.StockQuantity <= p.ReorderLevel && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Product>();

        var term = searchTerm.Trim().ToLower();
        var termNoSpace = term.Replace(" ", string.Empty);

        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive && (
                (p.Name != null && (p.Name.ToLower().Contains(term) || p.Name.ToLower().Contains(termNoSpace))) ||
                (p.Description != null && (p.Description.ToLower().Contains(term) || p.Description.ToLower().Contains(termNoSpace))) ||
                (p.SKU != null && p.SKU.ToLower().Contains(term)) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(term)) ||
                (p.Brand != null && p.Brand.Name.ToLower().Contains(term)) ||
                (p.Unit != null && p.Unit.ToLower().Contains(term))
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredProductsAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        List<Guid>? brandIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minDiscount = null,
        bool? inStock = null,
        bool? isFeatured = null,
        string sortBy = "name",
        bool sortDescending = false,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || 
                                    p.Description.Contains(searchTerm) ||
                                    p.Category.Name.Contains(searchTerm) ||
                                    p.Brand.Name.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandIds != null && brandIds.Any())
        {
            query = query.Where(p => brandIds.Contains(p.BrandId));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) <= maxPrice.Value);
        }

        if (minDiscount.HasValue)
        {
            query = query.Where(p => p.DiscountedPrice.HasValue && 
                                    ((p.Price - p.DiscountedPrice.Value) / p.Price * 100) >= minDiscount.Value);
        }

        if (inStock.HasValue && inStock.Value)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        if (isFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == isFeatured.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "price" => sortDescending 
                ? query.OrderByDescending(p => p.DiscountedPrice ?? p.Price)
                : query.OrderBy(p => p.DiscountedPrice ?? p.Price),
            "discount" => sortDescending
                ? query.OrderByDescending(p => p.DiscountedPrice.HasValue ? (p.Price - p.DiscountedPrice.Value) / p.Price * 100 : 0)
                : query.OrderBy(p => p.DiscountedPrice.HasValue ? (p.Price - p.DiscountedPrice.Value) / p.Price * 100 : 0),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "popularity" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.OrderItems.Count),
            _ => sortDescending 
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name)
        };

        // Apply pagination
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    public async Task<IEnumerable<Product>> GetRelatedProductsAsync(Guid productId, int limit = 4, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
            return Enumerable.Empty<Product>();

        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != productId && 
                       p.IsActive)
            .OrderBy(p => Guid.NewGuid()) // Random order
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string searchTerm, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<string>();

        var suggestions = await _context.Products
            .Where(p => p.IsActive && p.Name.Contains(searchTerm))
            .Select(p => p.Name)
            .Distinct()
            .Take(limit)
            .ToListAsync(cancellationToken);

        return suggestions;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(new Product { Id = id });
        return Task.CompletedTask;
    }
}
