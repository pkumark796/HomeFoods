using HomeFoods.Domain.Entities;

namespace HomeFoods.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    // Advanced filtering and pagination
    Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredProductsAsync(
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
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Product>> GetRelatedProductsAsync(Guid productId, int limit = 4, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetSearchSuggestionsAsync(string searchTerm, int limit = 10, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
