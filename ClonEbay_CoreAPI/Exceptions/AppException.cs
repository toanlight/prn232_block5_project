namespace ClonEbay_CoreAPI.Exceptions
{
    /// <summary>
    /// Base exception class cho tất cả các custom exceptions của ứng dụng (mặc định là 4xx / domain errors).
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public object? Errors { get; }

        public AppException(string message, int statusCode = 400, object? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }

        public AppException(string message, Exception innerException, int statusCode = 400, object? errors = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
