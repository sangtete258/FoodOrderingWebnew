namespace FoodOrderingWeb.Models.Entities
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int FoodId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Food? Food { get; set; }
    }
}