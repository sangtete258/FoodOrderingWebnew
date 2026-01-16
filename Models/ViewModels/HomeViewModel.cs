using FoodOrderingWeb.Models.Entities;

namespace FoodOrderingWeb.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Food> PopularFoods { get; set; } = new List<Food>();
    }
}