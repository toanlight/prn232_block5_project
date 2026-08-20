using Microsoft.AspNetCore.Mvc;
using ClonEbay_CoreAPI.Exceptions;

namespace ClonEbay_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestErrorController : ControllerBase
    {
        private readonly ILogger<TestErrorController> _logger;

        public TestErrorController(ILogger<TestErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Test 400 Bad Request
        /// </summary>
        [HttpGet("bad-request")]
        public IActionResult ThrowBadRequest()
        {
            throw new BadRequestException("Tham số truyền vào không hợp lệ hoặc thiếu dữ liệu bắt buộc.");
        }

        /// <summary>
        /// Test 401 Unauthorized
        /// </summary>
        [HttpGet("unauthorized")]
        public IActionResult ThrowUnauthorized()
        {
            throw new UnauthorizedException("Phiên làm việc của bạn đã hết hạn. Vui lòng đăng nhập lại.");
        }

        /// <summary>
        /// Test 403 Forbidden
        /// </summary>
        [HttpGet("forbidden")]
        public IActionResult ThrowForbidden()
        {
            throw new ForbiddenException("Tài khoản của bạn không có quyền Admin để thực hiện thao tác này.");
        }

        /// <summary>
        /// Test 404 Not Found
        /// </summary>
        [HttpGet("not-found")]
        public IActionResult ThrowNotFound()
        {
            throw new NotFoundException("Product", 9999);
        }

        /// <summary>
        /// Test 422 Validation Error
        /// </summary>
        [HttpGet("validation-error")]
        public IActionResult ThrowValidationError()
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email không đúng định dạng.", "Email đã được sử dụng." } },
                { "Price", new[] { "Giá sản phẩm phải lớn hơn 0." } }
            };

            throw new ValidationException(errors, "Dữ liệu form nhập không hợp lệ.");
        }

        /// <summary>
        /// Test 500 Unhandled Server Exception
        /// </summary>
        [HttpGet("server-error")]
        public IActionResult ThrowServerError()
        {
            // Giả lập lỗi null reference hoặc crash database không lường trước
            string? nullObject = null;
            _logger.LogInformation("Đang thử truy cập đối tượng null...");
            return Ok(nullObject!.Length);
        }
    }
}
