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
            context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
            context.Response.Headers.CacheControl = "no-store";
            context.Response.ContentType =
                "application/json";

            context.Response.StatusCode =
                exception is FarmStay.Application.Interfaces.Common.TenantAccessException denied ? denied.Status : (int)HttpStatusCode.InternalServerError;

            var response =
                new ApiResponse<string>
                {
                    Success = false,
                    Message = exception is FarmStay.Application.Interfaces.Common.TenantAccessException access ? access.Message : "An unexpected error occurred. Reference: " + context.TraceIdentifier,
                    Data = null
                };

            var jsonResponse =
                JsonSerializer.Serialize(response);

            return context.Response
                .WriteAsync(jsonResponse);
        }
    }
}