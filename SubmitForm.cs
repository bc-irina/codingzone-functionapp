using System;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
namespace Company.Function
{
    public static class SaveWebhook
    {
        [FunctionName("SubmitForm")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "submit")] HttpRequest webhookReq,
            [CosmosDB(
                databaseName: "codingzone-db",
                collectionName: "codingzone",
                ConnectionStringSetting = "CosmosDbConnectionString")]out dynamic  outputDocument,
            ILogger log)
        {
            log.LogInformation($"Webhook function processed a request at: {DateTime.Now}");
            string authHeader = webhookReq.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                //the coding should be iso or you could use ASCII and UTF-8 decoder
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var config = new ConfigurationBuilder()
                          .AddEnvironmentVariables()
                          .Build();
                string userNameKeyVault = Environment.GetEnvironmentVariable("WebHookAuth", EnvironmentVariableTarget.Process);
                log.LogInformation(usernamePassword);
                if (usernamePassword != userNameKeyVault)
                {
                    outputDocument = null;
                    return new UnauthorizedObjectResult("Unauthorized access Forbidden");
                }

                string requestBody = new StreamReader(webhookReq.Body).ReadToEnd();

                outputDocument = requestBody;
                try
                {
                    outputDocument = requestBody;
                }
                catch (Exception e)
                {
                    log.LogInformation(e.Message);
                    outputDocument = requestBody;
                }

                return (ActionResult)new OkObjectResult(requestBody);
            }
            else
            {
                outputDocument = null;
                return new UnauthorizedObjectResult("Unauthorized access Forbidden");
            }
        }
    }
}
