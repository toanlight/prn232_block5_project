namespace ClonEbay_CoreAPI.Exceptions
{
    /// <summary>
    /// Lỗi 404 Not Found - Dùng khi không tìm thấy tài nguyên (Entity, User, Product, Record...).
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message = "Không tìm thấy dữ liệu yêu cầu.")
            : base(message, 404)
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"Tài nguyên '{entityName}' với khóa ({key}) không tồn tại.", 404)
        {
        }
    }
}
