using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;

namespace nordelta.cobra.webapi.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly RelationalDbContext _context;

        public NotificationRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public List<NotificationType> GetNotificationTypes()
        {
            return _context.NotificationType
                .Include(x => x.NotificationTypeRoles).ThenInclude(y => y.Role)
                .Include(x => x.Delivery)
                .Include(x => x.Template)
                .ToList();
        }

        public NotificationType GetNotificationTypeById(int id)
        {
            return _context.NotificationType
                .Include(x => x.NotificationTypeRoles).ThenInclude(y => y.Role)
                .Include(x => x.Template)
                .Include(x => x.Delivery)
                .SingleOrDefault(x => x.Id == id);
        }

        public Template GetTemplateById(int id)
        {
            return _context.Template.SingleOrDefault(x => x.Id == id);
        }

        public void UpdateNotificationType(NotificationType notificationType)
        {
            _context.Update(notificationType);
            _context.SaveChanges();
        }

        public void UpdateTemplate(Template template)
        {
            _context.Update(template);
            _context.SaveChanges();
        }

        public void Save(Notification notification)
        {
            _context.Notification.Add(notification);
            _context.SaveChanges();
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            await _context.Notification.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public List<TemplateTokenReference> GetAllTemplateReferences()
        {
            return _context.TemplateTokenReference.ToList();
        }
        public Template GetTemplateByDescInternal(string description)
        {
            return _context.Template.SingleOrDefault(x => x.Description == description);
        }
    }
}
