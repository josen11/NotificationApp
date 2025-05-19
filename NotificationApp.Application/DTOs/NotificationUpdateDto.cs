namespace NotificationApp.Application.DTOs;
public class NotificationUpdateDto
{
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public string? WebLink { get; set; }
	public DateTime? ExpiresAtUtc { get; set; }
	public bool IsHighlighted { get; set; }
	public int? UpdatedByOracleEmployeeId { get; set; }
	public DateTimeOffset? UpdatedAtUtc { get; set; }
	public int NotificationTypeId { get; set; }
}
