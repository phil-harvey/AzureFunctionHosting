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
    internal class HostProxyWorkerRequest : SimpleWorkerRequest
    {
        private HttpCookieCollection _cookies;
        private readonly string _httpVerbName;
        private readonly byte[] _data;
        private readonly NameValueCollection _headers;
        private Uri _uri;

        public HttpContext HttpContext { get; set; }

        public override string GetServerName() { return _uri.Host; }

        public HostProxyWorkerRequest(Uri uri, TextWriter output, HttpCookieCollection cookies, string httpVerbName, byte[] data, NameValueCollection headers)
            : base(uri.AbsolutePath.TrimStart('/'), uri.Query.TrimStart('?'), output)
        {
            _uri = uri;
            _cookies = cookies;
            _httpVerbName = httpVerbName;
            _data = data;
            _headers = headers;
        }

        public override string GetHttpVerbName()
        {
            return _httpVerbName;
        }

        public override string GetKnownRequestHeader(int index)
        {
            switch (index)
            {
                case 0x19:
                    return MakeCookieHeader();
                default:
                    if (_headers == null)
                        return null;
                    return _headers[GetKnownRequestHeaderName(index)];
            }
        }

        public override string GetUnknownRequestHeader(string name)
        {
            if (_headers == null)
                return null;
            return _headers[name];
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            this.HttpContext = HttpContext.Current;

            if (_headers == null)
                return null;
            var unknownHeaders = from key in _headers.Keys.Cast<string>()
                                 let knownRequestHeaderIndex = GetKnownRequestHeaderIndex(key)
                                 where knownRequestHeaderIndex < 0
                                 select new[] { key, _headers[key] };
            return unknownHeaders.ToArray();
        }

        public override byte[] GetPreloadedEntityBody()
        {
            if (_data != null)
                return _data;
            else return base.GetPreloadedEntityBody();
        }

        public override bool IsSecure()
        {
            return this._uri.Scheme == "https";
        }

        private string MakeCookieHeader()
        {
            if ((_cookies == null) || (_cookies.Count == 0))
                return null;
            var sb = new StringBuilder();
            foreach (string cookieName in _cookies)
                sb.AppendFormat("{0}={1};", cookieName, _cookies[cookieName].Value);
            return sb.ToString();
        }
    }
}
