namespace HomeFoods.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public int? DiscountPercentage => DiscountedPrice.HasValue && Price > 0 
        ? (int)((Price - DiscountedPrice.Value) / Price * 100) 
        : null;

    // Related entities
    public CategoryDto? Category { get; set; }
    public BrandDto? Brand { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
}
