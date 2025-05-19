namespace NotificationApp.Application.DTOs;
public class ParticipantCreateDto
{
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public string? MiddleName { get; set; }
	public bool IsDeceased { get; set; }
	public bool IsVip { get; set; }
	public bool IsEscalated { get; set; }
	public List<int>? PartnerIds { get; set; }
	public List<int>? NotificationIds { get; set; }
}

