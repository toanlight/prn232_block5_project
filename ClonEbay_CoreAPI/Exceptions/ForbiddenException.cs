namespace ClonEbay_CoreAPI.Exceptions
{
    /// <summary>
    /// Lỗi 403 Forbidden - Dùng khi người dùng đã đăng nhập nhưng không có quyền truy cập tài nguyên.
    /// </summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Bạn không có quyền truy cập tài nguyên này.", object? errors = null)
            : base(message, 403, errors)
        {
        }
    }
}
