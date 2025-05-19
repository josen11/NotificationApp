using Microsoft.EntityFrameworkCore;
using NotificationApp.Application.Interfaces;
using NotificationApp.Infrastructure.Data;

namespace NotificationApp.Infrastructure.Repositories;
internal class GenericRepository<TEntity, TModel> : IGenericRepository<TEntity>
	where TEntity : class
	where TModel : class, new()
{
	protected readonly NotificationDbContext _ctx;
	private readonly DbSet<TModel> _db;
	private readonly Func<TModel, TEntity> _toDomain;
	private readonly Func<TEntity, TModel> _toModel;

	public GenericRepository(
		NotificationDbContext ctx,
		Func<TModel, TEntity> toDomain,
		Func<TEntity, TModel> toModel)
	{
		_ctx = ctx;
		_db = ctx.Set<TModel>();
		_toDomain = toDomain;
		_toModel = toModel;
	}

	public async Task AddAsync(TEntity entity, CancellationToken ct = default)
	{
		_db.Add(_toModel(entity));
		await _ctx.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(TEntity entity, CancellationToken ct = default)
	{
		_db.Remove(_toModel(entity));
		await _ctx.SaveChangesAsync(ct);
	}

	public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct = default)
	{
		var all = await _db.AsNoTracking().ToListAsync(ct);
		return all.Select(_toDomain).ToList();
	}

	public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
	{
		var model = await _db.FindAsync(new object[] { id }, ct);
		return model is null ? null : _toDomain(model);
	}

	public async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
	{
		_db.Update(_toModel(entity));
		await _ctx.SaveChangesAsync(ct);
	}
}
