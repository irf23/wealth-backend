using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

  namespace WealthBackend.Models
  {
      public class Asset
      {
          [Key]
          public string Id { get; set; } = string.Empty;

          public string AssetName { get; set; } = string.Empty;

          public string PrimaryAssetCategory { get; set; } = string.Empty;

          public string WealthAssetType { get; set; } = string.Empty;

          [Column(TypeName = "decimal(18,2)")]
          public decimal BalanceCurrent { get; set; }

          public DateTime BalanceAsOf { get; set; }

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

          public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

          // Navigation property
          public ICollection<AssetBalanceHistory> BalanceHistory { get; set; }
              = new List<AssetBalanceHistory>();
      }
  }