using FoodOrderingWeb.Models.Entities;

namespace FoodOrderingWeb.Models.ViewModels
{
    public class OrderFilterViewModel
    {
        public string? SearchString { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
    }
}