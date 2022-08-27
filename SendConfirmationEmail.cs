using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;



namespace Company.Function
{

    public static class SendConfirmationEmail
    {
        [FunctionName("SendConfirmationEmail")]
        public static async Task<SendGridMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "send/{email}")] HttpRequest req,
            ILogger log,
             [SendGrid(ApiKey = "SendGridApiKey")] IAsyncCollector<SendGridMessage> messageCollector,
             string email
             )
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

            messageCollector.AddAsync(msg);

            return msg;
        }
    }
}
