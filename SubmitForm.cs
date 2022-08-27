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
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;

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
            ILogger log,
            [SendGrid(ApiKey = "SendGridApiKey")] IAsyncCollector<SendGridMessage> messageCollector
            )

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

                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(Environment.GetEnvironmentVariable("SenderEmail")),
                    Subject = "Coding Zone new message",
                    PlainTextContent = requestBody

                };


                msg.AddTo(Environment.GetEnvironmentVariable("RecipientEmail"));



                messageCollector.AddAsync(msg);

                // var jsond = JsonConvert.SerializeObject(outputDocument);
                JObject json = JObject.Parse(requestBody);

                var age = json["age"];
                if (age != null)
                {
                    var email = json["email"].ToString();
                    msg = SendM(email);
                    messageCollector.AddAsync(msg);

                }



                return (ActionResult)new OkObjectResult(requestBody);
            }
            else
            {
                outputDocument = null;
                return new UnauthorizedObjectResult("Unauthorized access Forbidden");
            }
        }

        private static SendGridMessage SendM(string email)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(Environment.GetEnvironmentVariable("SenderEmail"), "Coding Zone Team"),
                Subject = "Coding Zone: Thanks for your registration",
            };

            var path = "email-template.html";
            string emailTemplate = File.ReadAllText(path);
            msg.AddContent("text/html", emailTemplate);


            msg.AddTo(email);

            return msg;


        }
    }
}
