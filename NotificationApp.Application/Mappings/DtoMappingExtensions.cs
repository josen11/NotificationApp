using NotificationApp.Application.DTOs;
using NotificationApp.Domain.Entities;

namespace NotificationApp.Application.Mappings;
public static class DtoMappingExtensions
{
	// Partner
	public static PartnerDto ToDto(this Partner p)
		=> new PartnerDto
		{
			PartnerId = p.PartnerId,
			PartnerName = p.PartnerName,
			EmailAddress = p.EmailAddress,
			ParticipantIds = p.Participants.Select(x => x.ParticipantId).ToList(),
			NotificationIds = p.Notifications.Select(x => x.NotificationId).ToList()
		};

	public static Partner ToDomain(this PartnerCreateDto dto)
		=> new Partner(0, dto.PartnerName, dto.EmailAddress);

	// Participant
	public static ParticipantDto ToDto(this Participant p)
		=> new ParticipantDto
		{
			ParticipantId = p.ParticipantId,
			FirstName = p.FirstName,
			LastName = p.LastName,
			MiddleName = p.MiddleName,
			IsDeceased = p.IsDeceased,
			IsVip = p.IsVip,
			IsEscalated = p.IsEscalated,
			PartnerIds = p.Partners.Select(x => x.PartnerId).ToList(),
			NotificationIds = p.Notifications.Select(x => x.NotificationId).ToList()
		};

	public static Participant ToDomain(this ParticipantCreateDto dto)
		=> new Participant(0, dto.FirstName, dto.LastName, dto.MiddleName,
						   dto.IsDeceased, dto.IsVip, dto.IsEscalated);

	// NotificationType
	public static NotificationTypeDto ToDto(this NotificationType t)
		=> new NotificationTypeDto { Id = t.Id, Name = t.Name };

	public static NotificationType ToDomain(this NotificationTypeCreateDto dto)
		=> new NotificationType(0, dto.Name);

	// Notification
	public static NotificationDto ToDto(this Notification n)
		=> new NotificationDto
		{
			NotificationId = n.NotificationId,
			Title = n.Title,
			Message = n.Message,
			WebLink = n.WebLink,
			ExpiresAtUtc = n.ExpiresAtUtc,
			IsHighlighted = n.IsHighlighted,
			CreatedByOracleEmployeeId = n.CreatedByOracleEmployeeId,
			CreatedAtUtc = n.CreatedAtUtc,
			UpdatedByOracleEmployeeId = n.UpdatedByOracleEmployeeId,
			UpdatedAtUtc = n.UpdatedAtUtc,
			NotificationTypeId = n.NotificationTypeId,
			ParticipantIds = n.Participants.Select(x => x.ParticipantId).ToList(),
			PartnerIds = n.Partners.Select(x => x.PartnerId).ToList()
		};

	public static Notification ToDomain(this NotificationCreateDto dto)
		=> new Notification(0,
			dto.Title, dto.Message, dto.WebLink, dto.ExpiresAtUtc,
			dto.IsHighlighted, dto.CreatedByOracleEmployeeId, dto.CreatedAtUtc,
			null, null, dto.NotificationTypeId);
}
