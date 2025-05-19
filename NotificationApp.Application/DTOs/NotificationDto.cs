namespace NotificationApp.Application.DTOs;
public class NotificationDto
{
	public int NotificationId { get; set; }
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public string? WebLink { get; set; }
	public DateTime? ExpiresAtUtc { get; set; }
	public bool IsHighlighted { get; set; }
	public int CreatedByOracleEmployeeId { get; set; }
	public DateTimeOffset CreatedAtUtc { get; set; }
	public int? UpdatedByOracleEmployeeId { get; set; }
	public DateTimeOffset? UpdatedAtUtc { get; set; }
	public int NotificationTypeId { get; set; }
	public List<int> ParticipantIds { get; set; } = new();
	public List<int> PartnerIds { get; set; } = new();
}
