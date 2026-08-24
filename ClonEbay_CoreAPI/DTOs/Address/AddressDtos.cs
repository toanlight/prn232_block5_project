using System.ComponentModel.DataAnnotations;

namespace ClonEbay_CoreAPI.DTOs.Address
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }
    }

    public class SaveAddressRequestDto
    {
        [Required(ErrorMessage = "Họ và tên người nhận không được để trống")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Số điện thoại phải từ 8 đến 20 ký tự")]
        [RegularExpression(@"^\+?[0-9\s().-]+$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống")]
        [StringLength(100, ErrorMessage = "Địa chỉ chi tiết tối đa 100 ký tự")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống")]
        [StringLength(50, ErrorMessage = "Tỉnh/Thành phố tối đa 50 ký tự")]
        public string City { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Quận/Huyện tối đa 50 ký tự")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Quốc gia không được để trống")]
        [StringLength(50, ErrorMessage = "Quốc gia tối đa 50 ký tự")]
        public string Country { get; set; } = "Việt Nam";

        [StringLength(20, ErrorMessage = "Mã bưu chính tối đa 20 ký tự")]
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; }
    }
}
