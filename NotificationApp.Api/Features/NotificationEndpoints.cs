using NotificationApp.Application.DTOs;
using NotificationApp.Application.Interfaces;
using NotificationApp.Application.Mappings;

namespace NotificationApp.Api.Features;

public static class NotificationEndpoints
{
	public static WebApplication MapNotificationEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/notifications");

		group.MapGet("/", async (INotificationRepository repo) =>
			(await repo.ListAsync()).Select(n => n.ToDto()));

		group.MapGet("/{id:int}", async (int id, INotificationRepository repo) =>
		{
			var n = await repo.GetByIdAsync(id);
			return n is not null
				? Results.Ok(n.ToDto())
				: Results.NotFound();
		});

		group.MapPost("/", async (NotificationCreateDto dto, INotificationRepository repo) =>
		{
			var entity = dto.ToDomain();
			await repo.AddAsync(entity);
			return Results.Created($"/notifications/{entity.NotificationId}", entity.ToDto());
		});

		group.MapPut("/{id:int}", async (int id, NotificationUpdateDto dto, INotificationRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			existing.ChangeTitle(dto.Title);
			existing.ChangeMessage(dto.Message);
			existing.ChangeWebLink(dto.WebLink);
			existing.SetExpiresAtUtc(dto.ExpiresAtUtc);
			if (dto.UpdatedByOracleEmployeeId.HasValue && dto.UpdatedAtUtc.HasValue)
			{
				existing.SetUpdatedMeta(dto.UpdatedByOracleEmployeeId.Value, dto.UpdatedAtUtc.Value);
			}
			await repo.UpdateAsync(existing);

			return Results.NoContent();
		});

		group.MapDelete("/{id:int}", async (int id, INotificationRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			await repo.DeleteAsync(existing);
			return Results.NoContent();
		});

		return app;
	}
}
