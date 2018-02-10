using PJH.Web.AzureFunctionHosting.HostProxy.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace PJH.Web.AzureFunctionHosting.HostProxy
{
    public class HostProxy : MarshalByRefObject, IRegisteredObject
    {
        public HostProxy()
        {
        }

        public RequestRespose RunRequest(Uri url, string httpVerbName, byte[] data, NameValueCollection headers)
        {
            HostProxyWorkerRequest request = null;
            string responseContent = null;

            using (var output = new StringWriter())
            {
                var cookies = CreateRequestCookies(headers);
                request = new HostProxyWorkerRequest(url, output, cookies, httpVerbName, data, headers);
                HttpRuntime.ProcessRequest(request);
                responseContent = output.ToString();
            }

            var responseHeaders = request.HttpContext.CreateResponseHeaders();
            var responseCookies = request.HttpContext.CreateResponseCookies();

            return new RequestRespose
            {
                Content = responseContent,
                StatusCode = request.HttpContext.Response.StatusCode,
                Headers = responseHeaders,
                ContentType = request.HttpContext.Response.ContentType,
                Cookies = responseCookies
            };

        }

        public void Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }

        private HttpCookieCollection CreateRequestCookies(NameValueCollection headers)
        {
            HttpCookieCollection cookies = new HttpCookieCollection();

            foreach (var key in headers.AllKeys)
            {
                var header = headers[key];

                if (key == "Cookie")
                {
                    var createCookieFromStringMethod = typeof(HttpRequest).GetMethod("CreateCookieFromString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    var cookie = (HttpCookie)createCookieFromStringMethod.Invoke(null, new string[] { header });
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }
    }
}
