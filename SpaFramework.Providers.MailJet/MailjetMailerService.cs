using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.Services.Functional;
using SpaFramework.Providers.MailerServiceAbstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Functional
{
    public class MailjetMailerService : MailerService, IMailerService
    {
        public MailjetMailerService(IConfiguration configuration, ILogger<MailjetMailerService> logger) : base(configuration, logger)
        {
        }

        protected async override Task SendEmailInternal(string toName, string toAddress, string fromName, string fromAddress, string subject, string text, string html)
        {
            JObject properties = new JObject
            {
                {
                    "From", new JObject
                    {
                        {"Email", fromAddress},
                        {"Name", fromName}
                    }
                },
                {
                    "To", new JArray
                    {
                        new JObject
                        {
                            {"Email", toAddress}
                        }
                    }
                },
                {
                    "Subject", subject
                },
                {
                    "TextPart", text
                }
            };

            if (html != null)
                properties["HTMLPart"] = html;

            MailjetClient client = new MailjetClient(_configuration["MailjetPublicKey"], _configuration["MailjetPrivateKey"])
            {
                Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.Messages, new JArray
            {
                properties
            });

            MailjetResponse response = await client.PostAsync(request);
            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Sending email {EmailAction} to {EmailAddress}: {EmaiSubject}", "succeeded", toAddress, subject);
            else
                _logger.LogError("Sending email {EmailAction} to {EmailAddress}: {EmailSubect}: {EmailResponseStatusCode}, {EmailResponseErrorInfo}, {EmailResponseErrorMessage}", "failed", toAddress, subject, response.StatusCode, response.GetErrorInfo(), response.GetErrorMessage());
        }
    }
}
