namespace HomeFoods.Application.DTOs;

public class SearchSuggestionDto
{
    public List<string> ProductNames { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<BrandDto> Brands { get; set; } = new();
}
