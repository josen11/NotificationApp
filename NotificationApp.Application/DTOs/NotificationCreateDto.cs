namespace NotificationApp.Application.DTOs;
public class NotificationCreateDto
{
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public string? WebLink { get; set; }
	public DateTime? ExpiresAtUtc { get; set; }
	public bool IsHighlighted { get; set; }
	public int CreatedByOracleEmployeeId { get; set; }
	public DateTimeOffset CreatedAtUtc { get; set; }
	public int NotificationTypeId { get; set; }
	public List<int>? ParticipantIds { get; set; }
	public List<int>? PartnerIds { get; set; }
}
