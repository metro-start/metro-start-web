using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

public static class LetsEncrypt
{
    [FunctionName("letsencrypt")]
    public static async Task<HttpResponseMessage> Run([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "letsencrypt/{code}")]HttpRequest req,
        string code,
        ILogger log)
    {
        log.LogInformation($"LetsEncrypt!");

        var content = File.ReadAllText(@"D:\home\site\wwwroot\.well-known\acme-challenge\" + code);
        var resp = new HttpResponseMessage(HttpStatusCode.OK);
        resp.Content = new StringContent(content, Encoding.UTF8, "text/plain");
        return resp;
    }
}