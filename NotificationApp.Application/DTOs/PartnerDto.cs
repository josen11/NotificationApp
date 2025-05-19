namespace NotificationApp.Application.DTOs;
public class PartnerDto
{
	public int PartnerId { get; set; }
	public string PartnerName { get; set; } = null!;
	public string? EmailAddress { get; set; }
	public List<int> ParticipantIds { get; set; } = new();
	public List<int> NotificationIds { get; set; } = new();
}
