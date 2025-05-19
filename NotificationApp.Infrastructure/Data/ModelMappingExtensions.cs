using DomainPartner = NotificationApp.Domain.Entities.Partner;
using ModelPartner = NotificationApp.Infrastructure.Data.Models.Partner;
using DomainParticipant = NotificationApp.Domain.Entities.Participant;
using ModelParticipant = NotificationApp.Infrastructure.Data.Models.Participant;
//using DomainPartnerParticipantMap = NotificationApp.Domain.Entities.PartnerParticipantIdentificationMap;
using DomainNotificationType = NotificationApp.Domain.Entities.NotificationType;
using ModelNotificationType = NotificationApp.Infrastructure.Data.Models.NotificationType;
using DomainNotification = NotificationApp.Domain.Entities.Notification;
using ModelNotification = NotificationApp.Infrastructure.Data.Models.Notification;
using DomainNotificationParticipant = NotificationApp.Domain.Entities.NotificationParticipant;
using DomainNotificationPartner = NotificationApp.Domain.Entities.NotificationPartner;

namespace NotificationApp.Infrastructure.Data;
internal static class ModelMappingExtensions
{
	// Partner
	public static DomainPartner ToDomain(this ModelPartner m)
		=> new DomainPartner(m.partnerId, m.partnerName, m.emailAddress);

	public static ModelPartner ToModel(this DomainPartner d)
		=> new ModelPartner
		{
			partnerId = d.PartnerId,
			partnerName = d.PartnerName,
			emailAddress = d.EmailAddress
		};

	// Participant
	public static DomainParticipant ToDomain(this ModelParticipant m)
		=> new DomainParticipant(
			m.participantId,
			m.firstName,
			m.lastName,
			m.middleName,
			m.isDeceased,
			m.isVIP,
			m.isEscalated
		);

	public static ModelParticipant ToModel(this DomainParticipant d)
		=> new ModelParticipant
		{
			participantId = d.ParticipantId,
			firstName = d.FirstName,
			lastName = d.LastName,
			middleName = d.MiddleName,
			isDeceased = d.IsDeceased,
			isVIP = d.IsVip,
			isEscalated = d.IsEscalated
		};

	// NotificationType
	public static DomainNotificationType ToDomain(this ModelNotificationType m)
		=> new DomainNotificationType(m.notificationTypeId, m.typeName);

	public static ModelNotificationType ToModel(this DomainNotificationType d)
		=> new ModelNotificationType
		{
			notificationTypeId = d.Id,
			typeName = d.Name
		};

	// Notification
	public static DomainNotification ToDomain(this ModelNotification m)
		=> new DomainNotification(
			m.notificationId,
			m.title,
			m.message,
			m.webLink,
			m.expiresAtUTC,
			m.isHighlighted ?? false,
			m.createdByOracleEmployeeId,
			m.createdAtUTC,
			m.updatedByOracleEmployeeId,
			m.updatedAtUTC,
			m.notificationTypeId ?? 0
		);

	public static ModelNotification ToModel(this DomainNotification d)
		=> new ModelNotification
		{
			notificationId = d.NotificationId,
			title = d.Title,
			message = d.Message,
			webLink = d.WebLink,
			expiresAtUTC = d.ExpiresAtUtc,
			isHighlighted = d.IsHighlighted,
			createdByOracleEmployeeId = d.CreatedByOracleEmployeeId,
			createdAtUTC = d.CreatedAtUtc,
			updatedByOracleEmployeeId = d.UpdatedByOracleEmployeeId,
			updatedAtUTC = d.UpdatedAtUtc,
			notificationTypeId = d.NotificationTypeId
		};
}
