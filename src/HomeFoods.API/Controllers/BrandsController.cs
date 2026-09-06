using Microsoft.AspNetCore.Mvc;
using HomeFoods.Domain.Repositories;
using HomeFoods.Application.DTOs;

namespace HomeFoods.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public BrandsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var brands = await _unitOfWork.Brands.GetAllAsync();

        var brandDtos = brands.Select(b => new BrandDto
        {
            Id = b.Id,
            Name = b.Name,
            LogoUrl = b.LogoUrl,
            Description = b.Description
        }).OrderBy(b => b.Name).ToList();

        return Ok(brandDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
            return NotFound();

        var brandDto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            LogoUrl = brand.LogoUrl,
            Description = brand.Description
        };

        return Ok(brandDto);
    }
}
