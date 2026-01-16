using System.ComponentModel.DataAnnotations;

namespace FoodOrderingWeb.Models.Entities
{
    public enum OrderStatus
    {
        [Display(Name = "Chờ xác nhận")]
        Pending = 0,

        [Display(Name = "Đang chuẩn bị")]
        Processing = 1,

        [Display(Name = "Đang giao")]
        Shipping = 2,

        [Display(Name = "Hoàn thành")]
        Completed = 3,

        [Display(Name = "Đã hủy")]
        Cancelled = 4
    }
}