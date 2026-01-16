using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingWeb.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CancellationReason { get; set; } // ✅ FIX: Đổi thành CancellationReason
        public DateTime? CancelledDate { get; set; } // ✅ THÊM: Ngày hủy

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalTotal { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public string Status
        {
            get => OrderStatus.ToString();
            set
            {
                if (!string.IsNullOrEmpty(value) && Enum.TryParse<OrderStatus>(value, out var status))
                {
                    OrderStatus = status;
                }
            }
        }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }

        // Navigation properties
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
        public virtual ICollection<OrderStatusHistory>? StatusHistories { get; set; } // ✅ THÊM HISTORY
    }
}