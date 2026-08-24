using System.ComponentModel.DataAnnotations;

namespace CloneEbay_FE.Models
{
    public class ReturnRequestViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReturnRequestViewModel
    {
        [Required(ErrorMessage = "Mã đơn hàng không được để trống.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do hoàn trả.")]
        [MaxLength(1000, ErrorMessage = "Lý do không vượt quá 1000 ký tự.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateReturnRequestStatusViewModel
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public string Status { get; set; } = string.Empty;
    }
}
