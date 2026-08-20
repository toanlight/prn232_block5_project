namespace ClonEbay_CoreAPI.Exceptions
{
    /// <summary>
    /// Lỗi 422 Unprocessable Entity - Dùng khi dữ liệu không vượt qua được các quy tắc kiểm tra (Validation Rules).
    /// </summary>
    public class ValidationException : AppException
    {
        public ValidationException(object validationErrors, string message = "Dữ liệu đầu vào không hợp lệ.")
            : base(message, 422, validationErrors)
        {
        }
    }
}
