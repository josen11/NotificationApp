using NotificationApp.Application.Interfaces;
using NotificationApp.Infrastructure.Data;
using DomainPartner = NotificationApp.Domain.Entities.Partner;
using ModelPartner = NotificationApp.Infrastructure.Data.Models.Partner;

namespace NotificationApp.Infrastructure.Repositories;
internal class PartnerRepository
	: GenericRepository<DomainPartner, ModelPartner>,
		IPartnerRepository
{
	public PartnerRepository(NotificationDbContext ctx)
		: base(ctx, m => m.ToDomain(), d => d.ToModel())
	{ }
}