using NotificationApp.Application.Interfaces;
using NotificationApp.Infrastructure.Data;
using DomainNotificationType = NotificationApp.Domain.Entities.NotificationType;
using ModelNotificationType = NotificationApp.Infrastructure.Data.Models.NotificationType;

namespace NotificationApp.Infrastructure.Repositories;
internal class NotificationTypeRepository
	: GenericRepository<DomainNotificationType, ModelNotificationType>,
		INotificationTypeRepository
{
	public NotificationTypeRepository(NotificationDbContext ctx)
		: base(ctx, m => m.ToDomain(), d => d.ToModel())
	{ }

	public Task<DomainNotificationType?> GetByNameAsync(string name, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
