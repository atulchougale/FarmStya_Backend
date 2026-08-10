using Microsoft.AspNetCore.Http;

namespace FarmStay.Application.Interfaces.Common
{
    public interface ITenantResolver
    {
        string GetCurrentDomain();
    }
}