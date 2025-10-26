using Microsoft.EntityFrameworkCore;
using WealthBackend.Data;
using WealthBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<WealthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DataImportService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// Data import endpoint
app.MapPost("/api/import", async (DataImportService importService) =>
{
    try
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "assets.json");
        var count = await importService.ImportAssetsFromJsonAsync(jsonPath);
        return Results.Ok(new { Message = $"Successfully imported {count} assets", Count = count });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Import failed"
        );
    }
})
.WithName("ImportAssets")
.WithDescription("Imports assets from JSON file and creates historical balance records");

// Get all assets
app.MapGet("/api/assets", async (WealthDbContext db) =>
{
    try
    {
        var assets = await db.Assets
            .Select(a => new
            {
                a.Id,
                a.AssetName,
                a.PrimaryAssetCategory,
                a.WealthAssetType,
                a.BalanceCurrent,
                a.BalanceAsOf
            })
            .ToListAsync();
        return Results.Ok(assets);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Failed to retrieve assets"
        );
    }
})
.WithName("GetAssets")
.WithDescription("Retrieves all assets with their current balances");

// Get asset by ID
app.MapGet("/api/assets/{id}", async (string id, WealthDbContext db) =>
{
    try
    {
        var asset = await db.Assets
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.AssetName,
                a.PrimaryAssetCategory,
                a.WealthAssetType,
                a.BalanceCurrent,
                a.BalanceAsOf
            })
            .FirstOrDefaultAsync();

        return asset is not null ? Results.Ok(asset) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Failed to retrieve asset"
        );
    }
})
.WithName("GetAssetById")
.WithDescription("Retrieves a single asset by its ID");

// Point-in-time historical balance query
app.MapGet("/api/assets/historical", async (DateTime asOfDate, WealthDbContext db) =>
{
    try
    {
        // Get all historical records on or before the target date
        var relevantHistory = await db.AssetBalanceHistories
            .Where(h => h.BalanceAsOf <= asOfDate)
            .Include(h => h.Asset)
            .ToListAsync();

        // Group by asset and get the most recent balance for each
        var historicalAssets = relevantHistory
            .GroupBy(h => h.AssetId)
            .Select(g =>
            {
                var latestHistory = g.OrderByDescending(h => h.BalanceAsOf).First();
                return new
                {
                    latestHistory.Asset.Id,
                    latestHistory.Asset.AssetName,
                    latestHistory.Asset.PrimaryAssetCategory,
                    latestHistory.Asset.WealthAssetType,
                    Balance = latestHistory.Balance,
                    BalanceAsOf = latestHistory.BalanceAsOf
                };
            })
            .ToList();

        return Results.Ok(historicalAssets);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Failed to retrieve historical balances"
        );
    }
})
.WithName("GetHistoricalAssets")
.WithDescription("Retrieves assets with balances as of a specific date. Returns the most recent balance on or before the specified date for each asset.");

app.Run();
