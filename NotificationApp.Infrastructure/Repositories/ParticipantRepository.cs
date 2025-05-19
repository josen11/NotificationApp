using NotificationApp.Application.Interfaces;
using NotificationApp.Infrastructure.Data;
using DomainParticipant = NotificationApp.Domain.Entities.Participant;
using ModelParticipant = NotificationApp.Infrastructure.Data.Models.Participant;
namespace NotificationApp.Infrastructure.Repositories;
internal class ParticipantRepository
	: GenericRepository<DomainParticipant, ModelParticipant>,
		IParticipantRepository
{
	public ParticipantRepository(NotificationDbContext ctx)
		: base(ctx, m => m.ToDomain(), d => d.ToModel())
	{ }
}
