using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.Providers.Twilio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SpaFramework.Providers.Twilio.Services
{
    public class TwilioSmsService : ITwilioSmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwilioSmsService> _logger;

        public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessage(long operatorId, TwilioSmsMessage twilioSmsMessage)
        {
            string whitelist = _configuration.GetValue<string>("Twilio:Whitelist");
            if (!string.IsNullOrEmpty(whitelist))
            {
                List<string> allowedPhoneNumbers = whitelist.Split(",").Select(x => x.ToLower()).ToList();
                if (!allowedPhoneNumbers.Contains(twilioSmsMessage.PhoneNumber))
                {
                    _logger.LogWarning("Discarding non-whitelisted phone number: {PhoneNumber}", twilioSmsMessage.PhoneNumber);
                    return;
                }
            }

            TwilioClient.Init(_configuration["Twilio:Operators:" + operatorId.ToString() + ":AccountSid"], _configuration["Twilio:Operators:" + operatorId.ToString() + ":AuthToken"]);

            var createMessageOptions = new CreateMessageOptions(new PhoneNumber(twilioSmsMessage.PhoneNumber))
            {
                Body = twilioSmsMessage.Message
            };
            createMessageOptions.MessagingServiceSid = _configuration["Twilio:Operators:" + operatorId.ToString() + ":MessagingServiceSid"];

            await MessageResource.CreateAsync(createMessageOptions);
        }
    }
}
