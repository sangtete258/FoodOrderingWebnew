namespace FoodOrderingWeb.Models.Entities
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderStatus FromStatus { get; set; } // ✅ FIX: Đổi từ OldStatus
        public OrderStatus ToStatus { get; set; }   // ✅ FIX: Đổi từ NewStatus
        public string? Note { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Order? Order { get; set; }
    }
}