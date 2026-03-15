namespace RestaurantFoodOrderingCustomer.Models
{
    public class CartItem
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
