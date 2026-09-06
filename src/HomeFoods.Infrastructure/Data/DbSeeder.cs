using HomeFoods.Domain.Entities;
using HomeFoods.Infrastructure.Data;

namespace HomeFoods.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(HomeFoodsDbContext context)
    {
        // Seed data if missing. Do not return early so we can fix missing image URLs on existing data.

        // Seed Guest User
        var guestUserId = new Guid("11111111-1111-1111-1111-111111111111");
        if (!context.Users.Any(u => u.Id == guestUserId))
        {
            var guestUser = new User
            {
                Id = guestUserId,
                FirstName = "Guest",
                LastName = "User",
                Email = "guest@homefoods.com",
                PhoneNumber = "0000000000",
                PasswordHash = "N/A", // Not used for guest
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(guestUser);
            await context.SaveChangesAsync();
        }

        // Seed Guest Address
        var guestAddressId = new Guid("22222222-2222-2222-2222-222222222222");
        if (!context.Addresses.Any(a => a.Id == guestAddressId))
        {
            var guestAddress = new Address
            {
                Id = guestAddressId,
                UserId = guestUserId,
                Street = "Guest Address",
                City = "Guest City",
                State = "Guest State",
                ZipCode = "000000",
                Country = "India",
                IsDefault = true,
                Type = AddressType.Home
            };
            context.Addresses.Add(guestAddress);
            await context.SaveChangesAsync();
        }

        // Seed Brands (only if none exist)
        if (!context.Brands.Any())
        {
            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), Name = "Organic Farms", Description = "100% Organic Products", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Brand { Id = Guid.NewGuid(), Name = "Spice World", Description = "Authentic Indian Spices", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Brand { Id = Guid.NewGuid(), Name = "Golden Harvest", Description = "Premium Grains & Rice", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Brand { Id = Guid.NewGuid(), Name = "Nature's Best", Description = "Natural & Healthy", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            context.Brands.AddRange(brands);
            await context.SaveChangesAsync();
        }

        // Seed Categories (only if none exist)
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Spices", Description = "All types of spices", IsActive = true, DisplayOrder = 1, ImageUrl = "https://images.unsplash.com/photo-1615485290382-441e4d049cb5?w=400" },
                new Category { Id = Guid.NewGuid(), Name = "Grains", Description = "Grains and cereals", IsActive = true, DisplayOrder = 2, ImageUrl = "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400" },
                new Category { Id = Guid.NewGuid(), Name = "Rice", Description = "Various types of rice", IsActive = true, DisplayOrder = 3, ImageUrl = "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=400" },
                new Category { Id = Guid.NewGuid(), Name = "Oils", Description = "Cooking oils", IsActive = true, DisplayOrder = 4, ImageUrl = "https://images.unsplash.com/photo-1508747703725-719777637510?w=400" },
                new Category { Id = Guid.NewGuid(), Name = "Pulses", Description = "Lentils and pulses", IsActive = true, DisplayOrder = 5, ImageUrl = "https://images.unsplash.com/photo-1599639957043-f3aa5c986398?w=400" },
                new Category { Id = Guid.NewGuid(), Name = "Nuts & Dry Fruits", Description = "Nuts and dried fruits", IsActive = true, DisplayOrder = 6, ImageUrl = "https://images.unsplash.com/photo-1508747703725-719777637510?w=400" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Ensure we have category and brand lists from the database (handles existing DB)
        var categoryList = context.Categories.OrderBy(c => c.DisplayOrder).ToList();
        var brandList = context.Brands.ToList();

        // Ensure existing categories have image URLs (update if missing)
        var categoryImageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Spices", "https://images.unsplash.com/photo-1615485290382-441e4d049cb5?w=400" },
            { "Grains", "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400" },
            { "Rice", "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=400" },
            { "Oils", "https://images.unsplash.com/photo-1508747703725-719777637510?w=400" },
            { "Pulses", "https://images.unsplash.com/photo-1599639957043-f3aa5c986398?w=400" },
            { "Nuts & Dry Fruits", "https://images.unsplash.com/photo-1508747703725-719777637510?w=400" }
        };

        var updated = false;
        foreach (var cat in categoryList)
        {
            if (string.IsNullOrWhiteSpace(cat.ImageUrl) && categoryImageMap.TryGetValue(cat.Name ?? string.Empty, out var img))
            {
                cat.ImageUrl = img;
                updated = true;
            }
        }

        if (updated)
        {
            await context.SaveChangesAsync();
            // refresh categoryList
            categoryList = context.Categories.OrderBy(c => c.DisplayOrder).ToList();
        }
        // Seed Products (only if none exist)
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Turmeric Powder",
                    Description = "Pure organic turmeric powder with high curcumin content",
                    SKU = "SP-001",
                    Barcode = "1234567890001",
                    Price = 5.99m,
                    DiscountedPrice = 4.99m,
                    Unit = "g",
                    Weight = 500,
                    StockQuantity = 100,
                    ReorderLevel = 20,
                    IsActive = true,
                    IsFeatured = true,
                    CategoryId = categoryList[0].Id,
                    BrandId = brandList[1].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1615485290382-441e4d049cb5?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Basmati Rice",
                    Description = "Premium aged basmati rice from India",
                    SKU = "RC-001",
                    Barcode = "1234567890002",
                    Price = 12.99m,
                    Unit = "kg",
                    Weight = 5,
                    StockQuantity = 50,
                    ReorderLevel = 10,
                    IsActive = true,
                    IsFeatured = true,
                    CategoryId = categoryList[2].Id,
                    BrandId = brandList[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Olive Oil",
                    Description = "Extra virgin olive oil, cold pressed",
                    SKU = "OL-001",
                    Barcode = "1234567890003",
                    Price = 18.99m,
                    DiscountedPrice = 15.99m,
                    Unit = "ml",
                    Weight = 1000,
                    StockQuantity = 75,
                    ReorderLevel = 15,
                    IsActive = true,
                    IsFeatured = true,
                    CategoryId = categoryList[3].Id,
                    BrandId = brandList[0].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Almonds",
                    Description = "Premium California almonds, raw and unsalted",
                    SKU = "NT-001",
                    Barcode = "1234567890004",
                    Price = 15.99m,
                    Unit = "g",
                    Weight = 500,
                    StockQuantity = 60,
                    ReorderLevel = 15,
                    IsActive = true,
                    IsFeatured = true,
                    CategoryId = categoryList[5].Id,
                    BrandId = brandList[3].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1508747703725-719777637510?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Chili Powder",
                    Description = "Hot and spicy red chili powder",
                    SKU = "SP-002",
                    Barcode = "1234567890005",
                    Price = 4.99m,
                    Unit = "g",
                    Weight = 250,
                    StockQuantity = 120,
                    ReorderLevel = 25,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[0].Id,
                    BrandId = brandList[1].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1599639957043-f3aa5c986398?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Split Red Lentils",
                    Description = "High protein red lentils, perfect for dal",
                    SKU = "PL-001",
                    Barcode = "1234567890006",
                    Price = 6.99m,
                    Unit = "kg",
                    Weight = 2,
                    StockQuantity = 80,
                    ReorderLevel = 20,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[4].Id,
                    BrandId = brandList[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1599639957043-f3aa5c986398?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Mustard Oil",
                    Description = "Cold-pressed mustard oil, ideal for Indian cooking",
                    SKU = "OL-002",
                    Barcode = "1234567890007",
                    Price = 9.49m,
                    Unit = "ml",
                    Weight = 500,
                    StockQuantity = 90,
                    ReorderLevel = 20,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[3].Id,
                    BrandId = brandList[0].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1543352634-8f0a0f3a0b2b?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Cumin Seeds (Jeera)",
                    Description = "Aromatic cumin seeds for tempering",
                    SKU = "SP-003",
                    Barcode = "1234567890008",
                    Price = 3.49m,
                    Unit = "g",
                    Weight = 200,
                    StockQuantity = 110,
                    ReorderLevel = 25,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[0].Id,
                    BrandId = brandList[1].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1585238342028-3e2b5e1b7d7b?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Sugar",
                    Description = "Refined white sugar, fine granules",
                    SKU = "GR-001",
                    Barcode = "1234567890009",
                    Price = 2.99m,
                    Unit = "kg",
                    Weight = 1,
                    StockQuantity = 200,
                    ReorderLevel = 50,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[1].Id,
                    BrandId = brandList[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1584270354949-8e2f3d6d8d8f?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Salt",
                    Description = "Iodized table salt",
                    SKU = "GR-002",
                    Barcode = "1234567890010",
                    Price = 0.99m,
                    Unit = "kg",
                    Weight = 1,
                    StockQuantity = 300,
                    ReorderLevel = 50,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[1].Id,
                    BrandId = brandList[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1505576391880-2a2b0c4b3b8b?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Cashews",
                    Description = "Premium cashew nuts, whole and roasted",
                    SKU = "NT-002",
                    Barcode = "1234567890011",
                    Price = 19.99m,
                    Unit = "g",
                    Weight = 500,
                    StockQuantity = 40,
                    ReorderLevel = 10,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[5].Id,
                    BrandId = brandList[3].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1598511720639-6b3b1a2d2b6a?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Moong Dal",
                    Description = "Split green gram, nutritious and versatile",
                    SKU = "PL-002",
                    Barcode = "1234567890012",
                    Price = 7.49m,
                    Unit = "kg",
                    Weight = 1,
                    StockQuantity = 120,
                    ReorderLevel = 25,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[4].Id,
                    BrandId = brandList[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1580910051078-2f8b6d3d1a1b?w=400"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Atta (Wheat Flour)",
                    Description = "Whole wheat flour for Indian rotis",
                    SKU = "GR-003",
                    Barcode = "1234567890013",
                    Price = 4.49m,
                    Unit = "kg",
                    Weight = 1,
                    StockQuantity = 150,
                    ReorderLevel = 30,
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = categoryList[1].Id,
                    BrandId = brandList[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1601004890684-d8cbf643f5f2?w=400"
                }
            };
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
