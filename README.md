# AzureFunctionHosting

Serverless ASP.NET hosting with Azure Functions!

An Azure Function to host ASP.NET websites as Azure Functions.

## Usage

1. Deploy PJH.Web.AzureFunctionHosting.HostProxy.dll into /bin folder of your asp.net website
2. Deploy ASP.NET site files into Azure Function storage sub folder
3. Create Azure Function:

**Note: the appDirectory parameter must point to the folder in Azure Function storage where you deployed the ASP.NET site files**

```
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
```
