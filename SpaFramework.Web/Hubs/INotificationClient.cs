using SpaFramework.App.Models.Notifications;
using System.Threading.Tasks;

namespace SpaFramework.Web.Hubs
{
    public interface INotificationClient
    {
        Task OnAlert(AlertDTO alert);
    }
}
