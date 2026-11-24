using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Portfolio.Infrastructure
{
    /// <summary>
    /// Yalnizca loopback (localhost) baglantilarindan erisime izin verir.
    /// Dis dunyadan gelen istek icin 404 doner: admin panelinin varligi bile
    /// disaridan gorunmez.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class LocalOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!IsLocal(context.HttpContext))
                context.Result = new NotFoundResult();
        }

        /// <summary>
        /// Istegin makinenin kendisinden gelip gelmedigini soyler. Header'a DEGIL,
        /// dogrudan baglanti adresine bakar.
        /// </summary>
        public static bool IsLocal(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            // RemoteIpAddress null ise (ornegin in-memory test sunucusu) istek
            // makinenin kendisinden geliyor kabul edilir.
            if (remoteIp == null)
                return true;

            if (remoteIp.IsIPv4MappedToIPv6)
                remoteIp = remoteIp.MapToIPv4();

            return IPAddress.IsLoopback(remoteIp);
        }
    }

    public static class LocalOnlyMiddlewareExtensions
    {
        /// <summary>
        /// [LocalOnly] tasiyan endpoint'leri kimlik dogrulamadan ONCE keser.
        /// MVC filtreleri authorization middleware'inden sonra kostugu icin yalnizca
        /// filtreye guvenirsek anonim bir uzak istek 404 yerine giris sayfasina 302
        /// alir ve admin panelinin varligini aciga cikarir. Hangi endpoint'in yerel
        /// oldugu bilgisi tek yerde, <see cref="LocalOnlyAttribute"/> niteliginde kalir.
        /// Bu katman <c>UseRouting()</c> ile <c>UseAuthentication()</c> arasina girer.
        /// </summary>
        public static IApplicationBuilder UseLocalOnlyEndpoints(this IApplicationBuilder app) =>
            app.Use(async (context, next) =>
            {
                var localOnly = context.GetEndpoint()?.Metadata.GetMetadata<LocalOnlyAttribute>();

                if (localOnly != null && !LocalOnlyAttribute.IsLocal(context))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await next();
            });
    }
}
