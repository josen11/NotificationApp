using NotificationApp.Application.Interfaces;
using NotificationApp.Infrastructure.Data;
using DomainNotification = NotificationApp.Domain.Entities.Notification;
using ModelNotification = NotificationApp.Infrastructure.Data.Models.Notification;

namespace NotificationApp.Infrastructure.Repositories;
internal class NotificationRepository
	: GenericRepository<DomainNotification, ModelNotification>,
		INotificationRepository
{
	public NotificationRepository(NotificationDbContext ctx)
		: base(ctx, m => m.ToDomain(), d => d.ToModel())
	{ }
}