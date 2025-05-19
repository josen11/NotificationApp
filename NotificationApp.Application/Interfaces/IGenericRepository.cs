using System.Linq.Expressions;

namespace NotificationApp.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
	Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
	Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
	Task AddAsync(T entity, CancellationToken ct = default);
	Task UpdateAsync(T entity, CancellationToken ct = default);
	Task DeleteAsync(T entity, CancellationToken ct = default);
}