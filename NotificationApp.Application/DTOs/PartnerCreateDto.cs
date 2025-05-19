
namespace NotificationApp.Application.DTOs;
public class PartnerCreateDto
{
	public string PartnerName { get; set; } = null!;
	public string? EmailAddress { get; set; }
	public List<int>? ParticipantIds { get; set; }
	public List<int>? NotificationIds { get; set; }
}
