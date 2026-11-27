using FarmStay.Application.Interfaces.Common;

namespace FarmStay.API.Middleware;

public sealed class PublicMediaBoundaryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IPublicMediaAccess media)
    {
        if (context.Request.Path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase))
        {
            if(context.Request.Method is not ("GET" or "HEAD")){context.Response.StatusCode=404;return;}
            try {if(!await media.CanReadAsync(context.Request.Path.Value??"",context.RequestAborted)){context.Response.StatusCode=404;return;}}
            catch(TenantAccessException){context.Response.StatusCode=404;return;}
            context.Response.Headers["X-Content-Type-Options"]="nosniff";
            context.Response.Headers["Cross-Origin-Resource-Policy"]="cross-origin";
            // Video byte-range reads must not reuse legacy sparse/opaque cache entries.
            // Access is still checked before StaticFiles handles each request.
            var extension=Path.GetExtension(context.Request.Path.Value??"").ToLowerInvariant();
            context.Response.Headers["Cache-Control"]=extension is ".mp4" or ".webm" or ".mov"
                ? "private, no-store" : "private, no-cache";
        }
        await next(context);
    }
}
