using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantFoodOrderingCustomer.Models
{
    public class Combo
    {
        [Key]
        public int ComboId { get; set; }

        [Required]
        [StringLength(200)]
        public string ComboName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal OriginalPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ComboPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Discount { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public bool IsVeg { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ValidUntil { get; set; }

        // Navigation property for combo items
        public virtual ICollection<ComboItem>? ComboItems { get; set; }
    }
}
