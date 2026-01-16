namespace FoodOrderingWeb.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        public decimal TotalAmount => Items.Sum(i => i.SubTotal);

        public int TotalItems => Items.Sum(i => i.Quantity);  // ← Dòng này PHẢI CÓ

        public decimal ShippingFee { get; set; } = 30000;

        public decimal FinalTotal => TotalAmount + ShippingFee;
    }
}