using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantFoodOrderingCustomer.Models
{
    public class ComboItem
    {
        [Key]
        public int ComboItemId { get; set; }

        [Required]
        public int ComboId { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }

        [Required]
        public int FoodId { get; set; }

        [ForeignKey("FoodId")]
        public virtual FoodItem? FoodItem { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;
    }
}
