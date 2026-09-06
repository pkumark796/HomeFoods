namespace HomeFoods.Application.DTOs;

public class ProductFilterDto
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? BrandIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinDiscount { get; set; }
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public string? SortBy { get; set; } = "name";
    public bool SortDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
