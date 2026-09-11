using AhmedOumezzine.EFCore.Repository.Extensions;
using EFCore.Repository.Demo.Data;
using EFCore.Repository.Demo.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));
builder.Services.AddDbContext<DemoDbContext>(o => o.UseSqlite("Data Source=demo.db"));
builder.Services.AddGenericRepository<DemoDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    db.Database.EnsureCreated();

    if (!db.Products.Any())
    {
        var categoryNames = new[]
        {
            "Electronics", "Computers", "Accessories", "Books", "Home", "Office",
            "Gaming", "Audio", "Photography", "Outdoor", "Fitness", "Travel"
        };
        var categories = categoryNames.Select(name => new Category { Name = name, Description = $"{name} products" }).ToArray();
        db.Categories.AddRange(categories);

        var names = new[] { "Laptop Pro", "Gaming Laptop", "Mechanical Keyboard", "Wireless Mouse", "4K Monitor", "Programming Book", "Desk Lamp", "Gaming Headset", "External SSD", "Office Chair" };
        var products = Enumerable.Range(1, 240).Select(index => new Product
        {
            Name = $"{names[(index - 1) % names.Length]} {index:00}",
            Sku = $"DEMO-{index:000}",
            Description = "Deterministic portfolio demo product",
            Price = 5 + (index * 37 % 2995),
            Stock = new[] { 0, 1, 2, 5, 10, 25, 50, 100 }[index % 8],
            IsActive = index % 7 != 0,
            Category = categories[(index - 1) % categories.Length],
            CreatedOnUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(-index)
        }).ToArray();
        db.Products.AddRange(products);
        db.SaveChanges();

        foreach (var product in products.Where(product => product.Stock == 0).Take(3))
        {
            product.IsDeleted = true;
            product.DeletedOnUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        db.SaveChanges();
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.Run();
