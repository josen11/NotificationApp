using NotificationApp.Application.DTOs;
using NotificationApp.Application.Interfaces;
using NotificationApp.Application.Mappings;

namespace NotificationApp.Api.Features;

public static class NotificationTypeEndpoints
{
	public static WebApplication MapNotificationTypeEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/notification-types");

		group.MapGet("/", async (INotificationTypeRepository repo) =>
			(await repo.ListAsync()).Select(t => t.ToDto()));

		group.MapGet("/{id:int}", async (int id, INotificationTypeRepository repo) =>
		{
			var t = await repo.GetByIdAsync(id);
			return t is not null
				? Results.Ok(t.ToDto())
				: Results.NotFound();
		});

		group.MapPost("/", async (NotificationTypeCreateDto dto, INotificationTypeRepository repo) =>
		{
			var entity = dto.ToDomain();
			await repo.AddAsync(entity);
			return Results.Created($"/notification-types/{entity.Id}", entity.ToDto());
		});

		group.MapPut("/{id:int}", async (int id, NotificationTypeUpdateDto dto, INotificationTypeRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			existing.ChangeName(dto.Name);
			await repo.UpdateAsync(existing);

			return Results.NoContent();
		});

		group.MapDelete("/{id:int}", async (int id, INotificationTypeRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			await repo.DeleteAsync(existing);
			return Results.NoContent();
		});

		return app;
	}
}
