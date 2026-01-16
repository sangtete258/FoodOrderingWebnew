using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingWeb.Models.Entities
{
    public class ShippingConfig
    {
        public int ShippingConfigId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khu vực")]
        [StringLength(100)]
        [Display(Name = "Tên khu vực")]
        public string AreaName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phí ship")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phí ship (VNĐ)")]
        [Range(0, 1000000, ErrorMessage = "Phí ship phải từ 0 đến 1,000,000 VNĐ")]
        public decimal ShippingFee { get; set; }

        [Display(Name = "Khoảng cách ước tính (km)")]
        [Range(0, 1000, ErrorMessage = "Khoảng cách phải từ 0 đến 1000 km")]
        public decimal? EstimatedDistance { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Từ khóa tìm kiếm")]
        [StringLength(500)]
        public string? SearchKeywords { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }
}