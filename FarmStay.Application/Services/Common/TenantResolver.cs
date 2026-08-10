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

        //public string GetCurrentDomain()
        //{
        //    var context = _httpContextAccessor.HttpContext;

        //    if (context == null)
        //        return string.Empty;

        //    // 1. Header (Postman / Swagger)
        //    var domain = context.Request.Headers["X-Domain"].FirstOrDefault();

        //    if (!string.IsNullOrWhiteSpace(domain))
        //        return domain;

        //    // 2. Current Host
        //    domain = context.Request.Host.Host;

        //    // 3. Local Development
        //    if (domain.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
        //        domain.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return "greenvalley.com";   // temporary development tenant
        //    }

        //    // 4. Production
        //    return domain;
        //}

        public string GetCurrentDomain()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return string.Empty;

            // Read Domain Header
            var domain = context.Request.Headers["X-Domain"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(domain))
                return string.Empty;

            return domain.Trim().ToLower();
        }
    }
}