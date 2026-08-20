namespace ClonEbay_CoreAPI.Exceptions
{
    /// <summary>
    /// Lỗi 401 Unauthorized - Dùng khi người dùng chưa đăng nhập hoặc token không hợp lệ/hết hạn.
    /// </summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Bạn chưa xác thực hoặc token không hợp lệ.", object? errors = null)
            : base(message, 401, errors)
        {
        }
    }
}
