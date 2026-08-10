using System.Net;
using System.Text.Json;
using FarmStay.Application.Common.ApiResponse;
using Serilog;

namespace FarmStay.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync( HttpContext context)
        {
            try
            {
                // Next middleware / controller
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);

                await HandleExceptionAsync(
                    context,
                    ex
                );
            }
        }

        private static Task HandleExceptionAsync( HttpContext context, Exception exception)
        {
            context.Response.ContentType =
                "application/json";

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            var response =
                new ApiResponse<string>
                {
                    Success = false,
                    Message = exception.Message,
                    Data = null
                };

            var jsonResponse =
                JsonSerializer.Serialize(response);

            return context.Response
                .WriteAsync(jsonResponse);
        }
    }
}