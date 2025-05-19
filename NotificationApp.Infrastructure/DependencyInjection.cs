using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationApp.Application.Interfaces;
using NotificationApp.Infrastructure.Data;
using NotificationApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NotificationApp.Infrastructure;
public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration config)
	{
		//services.AddDbContext<NotificationDbContext>(opts =>
		//	opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

		services.AddScoped<IPartnerRepository, PartnerRepository>();
		services.AddScoped<IParticipantRepository, ParticipantRepository>();
		services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
		services.AddScoped<INotificationRepository, NotificationRepository>();

		return services;
	}
}