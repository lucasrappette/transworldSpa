using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.Providers.MailerServiceAbstractions
{
    public interface IMailerService
    {
        Task SendEmail(string to, string subject, string text);
        Task SendEmail(string to, string subject, string text, string html);
        Task SendEmail(string toName, string toAddress, string subject, string text, string html);
    }
}
