using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PJH.Web.AzureFunctionHosting.HostProxy.Extensions
{
    public static class HttpContextExtensions
    {
        public static Dictionary<string, string> CreateResponseHeaders(this HttpContext httpContext)
        {
            var customHeaders = httpContext.Response.GetPrivateInstanceField<object>("_customHeaders") as System.Collections.ArrayList
                ?? httpContext.Response.GetPrivateInstanceField<object>("_headers") as System.Collections.ArrayList;

            var responseHeaders = new Dictionary<string, string>();

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                    responseHeaders.Add(header.GetPrivateInstanceProperty<string>("Name"), header.GetPrivateInstanceProperty<string>("Value"));
            }

            return responseHeaders;
        }

        public static List<string> CreateResponseCookies(this HttpContext httpContext)
        {
            var responseCookies = new List<string>();

            foreach (string cookieKey in httpContext.Response.Cookies.Keys)
            {
                var cookie = httpContext.Response.Cookies[cookieKey];
                var cookieHeader = $"{HttpUtility.UrlEncode(cookie.Name)}={HttpUtility.UrlEncode(cookie.Value)}";

                if (cookie.Expires > DateTime.MinValue)
                    cookieHeader += $"; Expires={cookie.Expires.ToString("R")}";

                if (!string.IsNullOrEmpty(cookie.Path))
                    cookieHeader += $"; Path={HttpUtility.UrlEncode(cookie.Path)}";

                if (!string.IsNullOrEmpty(cookie.Domain))
                    cookieHeader += $"; Domain={HttpUtility.UrlEncode(cookie.Domain)}";

                if (cookie.Secure)
                    cookieHeader += "; Secure";

                if (cookie.HttpOnly)
                    cookieHeader += "; HttpOnly";

                responseCookies.Add(cookieHeader);
            }

            return responseCookies;
        }
    }
}
