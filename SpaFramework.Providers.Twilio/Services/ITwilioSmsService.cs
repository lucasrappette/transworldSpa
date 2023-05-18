using SpaFramework.Providers.Twilio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.Providers.Twilio.Services
{
    public interface ITwilioSmsService
    {
        Task SendMessage(long operatorId, TwilioSmsMessage twilioSmsMessage);
    }
}
