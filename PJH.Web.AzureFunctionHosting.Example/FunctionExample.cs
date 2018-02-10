using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace PJH.Web.AzureFunctionHosting.Example
{
    public static class FunctionExample
    {
        [FunctionName("ProcessHttpRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*path}")]HttpRequestMessage req, TraceWriter log)
        {
            var rootDirectory = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.FullName;
            var appDirectory = Path.Combine(rootDirectory, "PJH.Web.AzureFunctionHosting.DemoSite");

            return 
                await PJH.Web.AzureFunctionHosting.Function.Run(
                    req,
                    appDirectory, 
                    log);
        }
    }
}
