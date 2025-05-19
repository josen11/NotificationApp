using NotificationApp.Domain;
using NotificationApp.Domain.Entities;

namespace NotificationApp.Application.Interfaces;

public interface INotificationTypeRepository : IGenericRepository<NotificationType>
{
	Task<NotificationType?> GetByNameAsync(string name, CancellationToken ct = default);
}