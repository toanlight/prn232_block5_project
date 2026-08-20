namespace ClonEbay_CoreAPI.Exceptions
{
    /// <summary>
    /// Lỗi 400 Bad Request - Dùng khi client gửi request không hợp lệ hoặc sai dữ liệu đầu vào.
    /// </summary>
    public class BadRequestException : AppException
    {
        public BadRequestException(string message = "Yêu cầu không hợp lệ (Bad Request).", object? errors = null)
            : base(message, 400, errors)
        {
        }
    }
}
