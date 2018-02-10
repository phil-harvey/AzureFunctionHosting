using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using PJH.Web.AzureFunctionHosting.HostProxy;

namespace PJH.Web.AzureFunctionHosting
{
    public static class Function
    {
        private const string APP_ID_PREFIX = "MvcControllerFunctionHost_";

        private static Guid? _appIdGuid = null;

        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string physicalDir, TraceWriter log)
        {
            if (!_appIdGuid.HasValue)
                _appIdGuid = new Guid();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            try
            {
                HttpResponseMessage response = null;

                var manager = ApplicationManager.GetApplicationManager();

                var path = typeof(HostProxy.HostProxy).AssemblyQualifiedName + " " + typeof(HostProxy.HostProxy).Assembly.CodeBase;

                string appId = APP_ID_PREFIX + _appIdGuid.GetHashCode().ToString("x");
                string virtualDir = "/";

                var requestUri = req.RequestUri;
                var url = $"{requestUri.Scheme}://{requestUri.Host}";

                if (requestUri.Port != 80 || requestUri.Port != 443)
                    url += $":{requestUri.Port}";

                url += requestUri.AbsolutePath;

                var queryString = requestUri.Query;

                if (queryString.StartsWith("?"))
                    queryString = queryString.Substring(1);

                var requestUrl = requestUri.AbsolutePath;

                if (requestUrl.Length > 1)
                {
                    // if local file exists on disk, just return it immediately
                    var filePath = physicalDir + requestUrl.Replace("/", "\\");

                    if(File.Exists(filePath))
                    {
                        response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.Content = new StringContent(File.ReadAllText(filePath));
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(requestUrl));
                        return response;
                    }
                }

                if (requestUrl.StartsWith("/"))
                    requestUrl = requestUrl.Substring(1);

                var hasAppDomain = manager.GetAppDomain(appId) != null;

                var host = (HostProxy.HostProxy)manager.CreateObject(appId, typeof(HostProxy.HostProxy), virtualDir, physicalDir, false, true);

                var headers = new NameValueCollection();

                foreach (var header in req.Headers)
                    foreach(var value in header.Value)
                        headers.Add(header.Key, value);

                foreach (var header in req.Content.Headers)
                    foreach (var value in header.Value)
                        headers.Add(header.Key, value);

                byte[] data = null;

                if(req.Content != null)
                    data = await req.Content.ReadAsByteArrayAsync();

                var requestRespose = host.RunRequest(req.RequestUri, req.Method.Method, data, headers);

                response = new HttpResponseMessage((HttpStatusCode)(requestRespose.StatusCode));
                response.Content = new StringContent(requestRespose.Content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(requestRespose.ContentType);

                foreach (var header in requestRespose.Headers)
                    response.Content.Headers.Add(header.Key, header.Value);

                foreach(var cookie in requestRespose.Cookies)
                    response.Content.Headers.Add("Set-Cookie", cookie);

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.Load(args.Name);
        }
    }
}
