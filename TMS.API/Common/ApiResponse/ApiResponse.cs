using Microsoft.AspNetCore.Mvc;
using System.Net;
using TMS.Application;

namespace TMS.API
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Messages { get; set; } = string.Empty;
        public T? Data { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public static class ApiResponse
    {
        public static IActionResult FromResult<T>(
            ControllerBase controller,
            ServiceResult<T> result)
        {
            if (result == null)
            {
                return controller.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Messages = "Unexpected null result",
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }

            var response = new ApiResponse<T>
            {
                Success = result.Success,
                Messages = result.Messages,
                Data = result.Data,
                StatusCode = result.StatusCode
            };

            return controller.StatusCode((int)result.StatusCode, response);
        }
    }
}
