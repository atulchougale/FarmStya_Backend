using FarmStay.Application.Interfaces.Common;
using Microsoft.AspNetCore.Http;

namespace FarmStay.Application.Services.Common
{
    public class TenantResolver : ITenantResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        
        public string GetCurrentDomain()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return string.Empty;

            var requested=context.Request.Headers["X-Domain"].FirstOrDefault();
            var host=context.Request.Host.Host.ToLowerInvariant();
            var origin=context.Request.Headers["Origin"].FirstOrDefault();
            // Image/video GETs commonly omit Origin when API and website use separate hosts.
            // This fallback is public-media-only; the resolved site still needs an active published DB mapping.
            if(string.IsNullOrWhiteSpace(origin)&&context.Request.Path.StartsWithSegments("/uploads")&&
                Uri.TryCreate(context.Request.Headers["Referer"].FirstOrDefault(),UriKind.Absolute,out var page)&&page.Scheme is "https" or "http")
                origin=page.GetLeftPart(UriPartial.Authority);
            var browserHost=Uri.TryCreate(origin,UriKind.Absolute,out var browser)?browser.Host.ToLowerInvariant():host;
            var candidate=string.IsNullOrWhiteSpace(requested)?browserHost:requested.Trim().ToLowerInvariant();
            if(Uri.CheckHostName(candidate)==UriHostNameType.Unknown)return string.Empty;
            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")=="Development" && host is "localhost" or "127.0.0.1") {
                var allowed=(Environment.GetEnvironmentVariable("FarmStay__DevelopmentPublicDomains")??"greenvalley.com").Split(';',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
                if(string.IsNullOrWhiteSpace(requested)&&browserHost is "localhost" or "127.0.0.1")return allowed.FirstOrDefault()??string.Empty;
                if(allowed.Contains(candidate,StringComparer.OrdinalIgnoreCase))return candidate;
            }
            if(candidate!=browserHost&&candidate!=host)return string.Empty;
            return candidate;

        }
    }
}