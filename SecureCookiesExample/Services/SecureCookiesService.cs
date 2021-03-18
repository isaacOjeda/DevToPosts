using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SecureCookiesExample.Services
{
    public class SecureCookiesService
    {
        private readonly IDataProtector _dataProtector;
        private readonly HttpContext _http;

        public SecureCookiesService(
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(SecureCookiesService));
            _http = httpContextAccessor.HttpContext;
        }

        public T GetCookieValue<T>(string name)
        {
            if (_http.Request.Cookies.ContainsKey(name))
            {
                var secureJson = _http.Request.Cookies[name];
                var rawJson = _dataProtector.Unprotect(secureJson);

                return JsonSerializer.Deserialize<T>(rawJson);
            }

            return default;
        }

        public void CreateCookie<T>(string name, T data)
        {
            var rawJson = JsonSerializer.Serialize(data);
            var secureJson = _dataProtector.Protect(rawJson);

            _http.Response.Cookies.Append(name, secureJson, new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });
        }
    }
}