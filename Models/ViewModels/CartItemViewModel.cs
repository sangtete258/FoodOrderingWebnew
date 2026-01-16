namespace FoodOrderingWeb.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public int MaxQuantity { get; set; } = 99; // Số lượng tối đa cho phép
        public decimal SubTotal => Price * Quantity;
    }
}