using FoodOrderingWeb.Models.Entities;

namespace FoodOrderingWeb.Models.ViewModels
{
    public class MenuViewModel
    {
        public IEnumerable<Food> Foods { get; set; } = Enumerable.Empty<Food>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int? CategoryId { get; set; }
        public int? SelectedCategoryId { get; set; }

        public string PriceRange { get; set; } = string.Empty;
        public bool? Available { get; set; }
        public string SearchString { get; set; } = string.Empty;

        // Thêm thuộc tính Categories để hiển thị danh mục bên sidebar
        public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();

        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }

    public class FoodDetailsViewModel
    {
        public Food Food { get; set; } = null!;
        public IEnumerable<Food> RelatedFoods { get; set; } = Enumerable.Empty<Food>();
    }
}
