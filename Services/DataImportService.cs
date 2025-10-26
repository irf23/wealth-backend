using System.Text.Json;
using WealthBackend.Data;
using WealthBackend.Models;

namespace WealthBackend.Services
{
    public class DataImportService
    {
        private readonly WealthDbContext _context;
        private readonly ILogger<DataImportService> _logger;

        public DataImportService(WealthDbContext context, ILogger<DataImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> ImportAssetsFromJsonAsync(string jsonFilePath)
        {
            try
            {
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var jsonAssets = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);

                if (jsonAssets == null || !jsonAssets.Any())
                {
                    _logger.LogWarning("No assets found in JSON file");
                    return 0;
                }

                var importedCount = 0;

                foreach (var jsonAsset in jsonAssets)
                {
                    var assetId = jsonAsset.GetProperty("assetId").GetString() ?? string.Empty;

                    // Skip if asset already exists
                    if (await _context.Assets.FindAsync(assetId) != null)
                        continue;

                    var asset = new Asset
                    {
                        Id = assetId,
                        AssetName = GetStringProperty(jsonAsset, "nickname"),
                        PrimaryAssetCategory = GetStringProperty(jsonAsset, "primaryAssetCategory"),
                        WealthAssetType = GetStringProperty(jsonAsset, "wealthAssetType"),
                        BalanceCurrent = GetDecimalProperty(jsonAsset, "balanceCurrent"),
                        BalanceAsOf = GetDateTimeProperty(jsonAsset, "balanceAsOf"),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Assets.Add(asset);

                    // Create initial historical balance record
                    var historyRecord = new AssetBalanceHistory
                    {
                        AssetId = asset.Id,
                        Balance = asset.BalanceCurrent,
                        BalanceAsOf = asset.BalanceAsOf,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AssetBalanceHistories.Add(historyRecord);

                    // Create additional historical records for testing point-in-time queries
                    await CreateSampleHistoricalRecords(asset);

                    importedCount++;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully imported {Count} assets", importedCount);

                return importedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing assets from JSON");
                throw;
            }
        }

        private async Task CreateSampleHistoricalRecords(Asset asset)
        {
            // Create 2 historical records for testing point-in-time queries
            var baseDate = asset.BalanceAsOf;

            // Record from 30 days ago
            var history1 = new AssetBalanceHistory
            {
                AssetId = asset.Id,
                Balance = asset.BalanceCurrent * 0.90m, // 90% of current value
                BalanceAsOf = baseDate.AddDays(-30),
                CreatedAt = DateTime.UtcNow
            };
            _context.AssetBalanceHistories.Add(history1);

            // Record from 60 days ago
            var history2 = new AssetBalanceHistory
            {
                AssetId = asset.Id,
                Balance = asset.BalanceCurrent * 0.80m, // 80% of current value
                BalanceAsOf = baseDate.AddDays(-60),
                CreatedAt = DateTime.UtcNow
            };
            _context.AssetBalanceHistories.Add(history2);
        }

        private string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                return property.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        private decimal GetDecimalProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.Number)
            {
                return property.GetDecimal();
            }
            return 0m;
        }

        private DateTime GetDateTimeProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                var dateString = property.GetString();
                if (DateTime.TryParse(dateString, out var date))
                {
                    return date;
                }
            }
            return DateTime.UtcNow;
        }
    }
}
