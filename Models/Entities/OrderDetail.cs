using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingWeb.Models.Entities
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal => Quantity * UnitPrice;

        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual Food? Food { get; set; }
    }
}