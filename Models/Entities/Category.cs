using System.ComponentModel.DataAnnotations;

namespace FoodOrderingWeb.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Food>? Foods { get; set; }
    }
}