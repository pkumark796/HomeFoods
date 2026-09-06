using Microsoft.AspNetCore.Mvc;
using HomeFoods.Domain.Repositories;
using HomeFoods.Application.DTOs;

namespace HomeFoods.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetFilteredProductsAsync(
            searchTerm: filter.SearchTerm,
            categoryId: filter.CategoryId,
            brandIds: filter.BrandIds,
            minPrice: filter.MinPrice,
            maxPrice: filter.MaxPrice,
            minDiscount: filter.MinDiscount,
            inStock: filter.InStock,
            isFeatured: filter.IsFeatured,
            sortBy: filter.SortBy ?? "name",
            sortDescending: filter.SortDescending,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize
        );

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountedPrice = p.DiscountedPrice,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            Unit = p.Unit,
            Weight = p.Weight,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive,
            SKU = p.SKU,
            Origin = p.Origin,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name,
                Description = p.Category.Description,
                ImageUrl = p.Category.ImageUrl,
                DisplayOrder = p.Category.DisplayOrder
            } : null,
            Brand = p.Brand != null ? new BrandDto
            {
                Id = p.Brand.Id,
                Name = p.Brand.Name,
                LogoUrl = p.Brand.LogoUrl,
                Description = p.Brand.Description
            } : null
        }).ToList();

        var response = new PaginatedResponseDto<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountedPrice = product.DiscountedPrice,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            Unit = product.Unit,
            Weight = product.Weight,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive,
            SKU = product.SKU,
            Origin = product.Origin,
            Category = product.Category != null ? new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Description = product.Category.Description,
                ImageUrl = product.Category.ImageUrl,
                DisplayOrder = product.Category.DisplayOrder
            } : null,
            Brand = product.Brand != null ? new BrandDto
            {
                Id = product.Brand.Id,
                Name = product.Brand.Name,
                LogoUrl = product.Brand.LogoUrl,
                Description = product.Brand.Description
            } : null
        };

        return Ok(productDto);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var products = await _unitOfWork.Products.GetFeaturedProductsAsync();

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountedPrice = p.DiscountedPrice,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            Unit = p.Unit,
            Weight = p.Weight,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive,
            SKU = p.SKU,
            Origin = p.Origin,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name,
                Description = p.Category.Description,
                ImageUrl = p.Category.ImageUrl,
                DisplayOrder = p.Category.DisplayOrder
            } : null,
            Brand = p.Brand != null ? new BrandDto
            {
                Id = p.Brand.Id,
                Name = p.Brand.Name,
                LogoUrl = p.Brand.LogoUrl,
                Description = p.Brand.Description
            } : null
        }).ToList();

        return Ok(productDtos);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var products = await _unitOfWork.Products.GetByCategoryIdAsync(categoryId);

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountedPrice = p.DiscountedPrice,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            Unit = p.Unit,
            Weight = p.Weight,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive,
            SKU = p.SKU,
            Origin = p.Origin,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name,
                Description = p.Category.Description,
                ImageUrl = p.Category.ImageUrl,
                DisplayOrder = p.Category.DisplayOrder
            } : null,
            Brand = p.Brand != null ? new BrandDto
            {
                Id = p.Brand.Id,
                Name = p.Brand.Name,
                LogoUrl = p.Brand.LogoUrl,
                Description = p.Brand.Description
            } : null
        }).ToList();

        return Ok(productDtos);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search term is required");

        var products = await _unitOfWork.Products.SearchAsync(q);

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountedPrice = p.DiscountedPrice,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            Unit = p.Unit,
            Weight = p.Weight,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive,
            SKU = p.SKU,
            Origin = p.Origin,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name,
                Description = p.Category.Description,
                ImageUrl = p.Category.ImageUrl,
                DisplayOrder = p.Category.DisplayOrder
            } : null,
            Brand = p.Brand != null ? new BrandDto
            {
                Id = p.Brand.Id,
                Name = p.Brand.Name,
                LogoUrl = p.Brand.LogoUrl,
                Description = p.Brand.Description
            } : null
        }).ToList();

        return Ok(productDtos);
    }

    [HttpGet("{id}/related")]
    public async Task<IActionResult> GetRelated(Guid id, [FromQuery] int limit = 4)
    {
        var products = await _unitOfWork.Products.GetRelatedProductsAsync(id, limit);

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountedPrice = p.DiscountedPrice,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            Unit = p.Unit,
            Weight = p.Weight,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive,
            SKU = p.SKU,
            Origin = p.Origin,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name,
                Description = p.Category.Description,
                ImageUrl = p.Category.ImageUrl,
                DisplayOrder = p.Category.DisplayOrder
            } : null,
            Brand = p.Brand != null ? new BrandDto
            {
                Id = p.Brand.Id,
                Name = p.Brand.Name,
                LogoUrl = p.Brand.LogoUrl,
                Description = p.Brand.Description
            } : null
        }).ToList();

        return Ok(productDtos);
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<string>());

        var suggestions = await _unitOfWork.Products.GetSearchSuggestionsAsync(q, limit);
        return Ok(suggestions);
    }
}
