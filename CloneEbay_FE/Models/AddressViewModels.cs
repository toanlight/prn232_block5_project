using System.ComponentModel.DataAnnotations;

namespace CloneEbay_FE.Models
{
    public class AddressViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên người nhận")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Số điện thoại phải từ 8 đến 20 ký tự")]
        [RegularExpression(@"^\+?[0-9\s().-]+$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết")]
        [StringLength(100, ErrorMessage = "Địa chỉ chi tiết tối đa 100 ký tự")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Tỉnh/Thành phố")]
        [StringLength(50, ErrorMessage = "Tỉnh/Thành phố tối đa 50 ký tự")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string City { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Quận/Huyện tối đa 50 ký tự")]
        [Display(Name = "Quận/Huyện")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập quốc gia")]
        [StringLength(50, ErrorMessage = "Quốc gia tối đa 50 ký tự")]
        [Display(Name = "Quốc gia")]
        public string Country { get; set; } = "Việt Nam";

        [StringLength(20, ErrorMessage = "Mã bưu chính tối đa 20 ký tự")]
        [Display(Name = "Mã bưu chính")]
        public string? PostalCode { get; set; }

        [Display(Name = "Đặt làm địa chỉ mặc định")]
        public bool IsDefault { get; set; }
    }
}
