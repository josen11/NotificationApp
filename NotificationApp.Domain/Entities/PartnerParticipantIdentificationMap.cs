namespace NotificationApp.Domain.Entities;
public sealed class PartnerParticipantIdentificationMap
{
	public int PartnerId { get; private set; }
	public int ParticipantId { get; private set; }

	internal PartnerParticipantIdentificationMap() { }

	public PartnerParticipantIdentificationMap(int partnerId, int participantId)
	{
		PartnerId = partnerId;
		ParticipantId = participantId;
	}
}