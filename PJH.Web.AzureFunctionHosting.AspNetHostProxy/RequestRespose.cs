using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PJH.Web.AzureFunctionHosting.HostProxy
{
    [Serializable]
    public class RequestRespose
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public List<string> Cookies { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

}
