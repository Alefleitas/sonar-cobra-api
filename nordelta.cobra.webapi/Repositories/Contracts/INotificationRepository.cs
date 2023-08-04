using nordelta.cobra.webapi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface INotificationRepository
    {
        List<NotificationType> GetNotificationTypes();
        void Save(Notification notification);
        NotificationType GetNotificationTypeById(int id);
        Template GetTemplateById(int id);
        void UpdateNotificationType(NotificationType notificationType);
        void UpdateTemplate(Template template);
        List<TemplateTokenReference> GetAllTemplateReferences();
        Template GetTemplateByDescInternal(string description);
        Task<Notification> AddAsync(Notification notification);
    }
}