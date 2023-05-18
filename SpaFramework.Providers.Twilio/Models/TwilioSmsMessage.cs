using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.Providers.Twilio.Models
{
    public class TwilioSmsMessage
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
    }
}
