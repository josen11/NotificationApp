using NotificationApp.Application.DTOs;
using NotificationApp.Application.Interfaces;
using NotificationApp.Application.Mappings;

namespace NotificationApp.Api.Features;

public static class PartnerEndpoints
{
	public static WebApplication MapPartnerEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/partners");

		group.MapGet("/", async (IPartnerRepository repo) =>
			(await repo.ListAsync()).Select(p => p.ToDto()));

		group.MapGet("/{id:int}", async (int id, IPartnerRepository repo) =>
		{
			var p = await repo.GetByIdAsync(id);
			return p is not null
				? Results.Ok(p.ToDto())
				: Results.NotFound();
		});

		group.MapPost("/", async (PartnerCreateDto dto, IPartnerRepository repo) =>
		{
			var entity = dto.ToDomain();
			await repo.AddAsync(entity);
			return Results.Created($"/partners/{entity.PartnerId}", entity.ToDto());
		});

		group.MapPut("/{id:int}", async (int id, PartnerUpdateDto dto, IPartnerRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			existing.ChangeName(dto.PartnerName);
			existing.ChangeEmail(dto.EmailAddress);
			await repo.UpdateAsync(existing);

			return Results.NoContent();
		});

		group.MapDelete("/{id:int}", async (int id, IPartnerRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			await repo.DeleteAsync(existing);
			return Results.NoContent();
		});

		return app;
	}
}