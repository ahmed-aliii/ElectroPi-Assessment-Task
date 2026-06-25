using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Application
{
    public class ServiceResult<T>
    {
        public bool Success { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public string Messages { get; init; } = string.Empty;
        public T? Data { get; init; }

        #region Success Results

        public static ServiceResult<T> Ok(
            T data,
            string message = "Success")
            => new()
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Data = data,
                Messages = message
            };

        public static ServiceResult<T> Created(
            T data,
            string message = "Created successfully")
            => new()
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Data = data,
                Messages = message
            };

        public static ServiceResult<T> Accepted(
            string message = "Accepted")
            => new()
            {
                Success = true,
                StatusCode = HttpStatusCode.Accepted,
                Messages = message
            };

        public static ServiceResult<T> NoContent(
            string message = "No content")
            => new()
            {
                Success = true,
                StatusCode = HttpStatusCode.NoContent,
                Messages = message
            };

        #endregion

        #region Failure Results

        public static ServiceResult<T> BadRequest(
            string message,
            IEnumerable<string>? errors = null)
            => new()
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Messages = message,
            };

        public static ServiceResult<T> BadRequestWithData(
            T data,
          string message,
          IEnumerable<string>? errors = null)
          => new()
          {
              Success = false,
              StatusCode = HttpStatusCode.BadRequest,
              Messages = message,
              Data = data
          };

        public static ServiceResult<T> Unauthorized(
            string message = "Unauthorized")
            => new()
            {
                Success = false,
                StatusCode = HttpStatusCode.Unauthorized,
                Messages = message
            };

        public static ServiceResult<T> Forbidden(
            string message = "Forbidden")
            => new()
            {
                Success = false,
                StatusCode = HttpStatusCode.Forbidden,
                Messages = message
            };

        public static ServiceResult<T> NotFound(
            string message = "Not found")
            => new()
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Messages = message
            };

        public static ServiceResult<T> Conflict(
            string message)
            => new()
            {
                Success = false,
                StatusCode = HttpStatusCode.Conflict,
                Messages = message
            };

        public static ServiceResult<T> Failure(
            string message = "Internal server error")
            => new()
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Messages = message
            };

        #endregion
    }
}
