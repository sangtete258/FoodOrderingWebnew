using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingWeb.Models.Entities
{
    public class Food
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]  // Thêm định nghĩa này
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int ViewCount { get; set; }
        public virtual Category? Category { get; set; }
    }
}