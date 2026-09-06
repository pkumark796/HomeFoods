using Microsoft.AspNetCore.Mvc;
using HomeFoods.Infrastructure.Data;
using HomeFoods.Domain.Entities;

namespace HomeFoods.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly HomeFoodsDbContext _context;

    public SeedController(HomeFoodsDbContext context)
    {
        _context = context;
    }

    [HttpPost("guest-user")]
    public async Task<IActionResult> SeedGuestUser()
    {
        var guestUserId = new Guid("11111111-1111-1111-1111-111111111111");

        // Check if guest user already exists
        var existingUser = await _context.Users.FindAsync(guestUserId);
        if (existingUser != null)
        {
            return Ok(new { message = "Guest user already exists" });
        }

        var guestUser = new User
        {
            Id = guestUserId,
            FirstName = "Guest",
            LastName = "User",
            Email = "guest@homefoods.com",
            PhoneNumber = "0000000000",
            PasswordHash = "N/A",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(guestUser);
        await _context.SaveChangesAsync();

        var guestAddress = new Address
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            UserId = guestUserId,
            Street = "Guest Address",
            City = "Guest City",
            State = "Guest State",
            ZipCode = "000000",
            Country = "India",
            IsDefault = true,
            Type = AddressType.Home
        };
        _context.Addresses.Add(guestAddress);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Guest user and address created successfully", userId = guestUserId });
    }
}
