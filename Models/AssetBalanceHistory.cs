  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  namespace WealthBackend.Models
  {
      public class AssetBalanceHistory
      {
          [Key]
          public int Id { get; set; }

          [Required]
          public string AssetId { get; set; } = string.Empty;

          [Column(TypeName = "decimal(18,2)")]
          public decimal Balance { get; set; }

          [Required]
          public DateTime BalanceAsOf { get; set; }

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

          // Navigation property
          [ForeignKey(nameof(AssetId))]
          public Asset Asset { get; set; } = null!;
      }
  }