using NotificationApp.Application.DTOs;
using NotificationApp.Application.Interfaces;
using NotificationApp.Application.Mappings;

namespace NotificationApp.Api.Features;

public static class ParticipantEndpoints
{
	public static WebApplication MapParticipantEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/participants");

		group.MapGet("/", async (IParticipantRepository repo) =>
			(await repo.ListAsync()).Select(p => p.ToDto()));

		group.MapGet("/{id:int}", async (int id, IParticipantRepository repo) =>
		{
			var p = await repo.GetByIdAsync(id);
			return p is not null
				? Results.Ok(p.ToDto())
				: Results.NotFound();
		});

		group.MapPost("/", async (ParticipantCreateDto dto, IParticipantRepository repo) =>
		{
			var entity = dto.ToDomain();
			await repo.AddAsync(entity);
			return Results.Created($"/participants/{entity.ParticipantId}", entity.ToDto());
		});

		group.MapPut("/{id:int}", async (int id, ParticipantUpdateDto dto, IParticipantRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			existing.ChangeFirstName(dto.FirstName);
			existing.ChangeLastName(dto.LastName);
			existing.ChangeMiddleName(dto.MiddleName);
			if (dto.IsDeceased) existing.MarkDeceased();
			if (dto.IsVip) existing.MarkVip();
			if (dto.IsEscalated) existing.MarkEscalated();
			await repo.UpdateAsync(existing);

			return Results.NoContent();
		});

		group.MapDelete("/{id:int}", async (int id, IParticipantRepository repo) =>
		{
			var existing = await repo.GetByIdAsync(id);
			if (existing is null) return Results.NotFound();

			await repo.DeleteAsync(existing);
			return Results.NoContent();
		});

		return app;
	}
}
