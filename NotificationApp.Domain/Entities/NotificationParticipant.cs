namespace NotificationApp.Domain.Entities;
public sealed class NotificationParticipant
{
	public int NotificationId { get; private set; }
	public int ParticipantId { get; private set; }

	internal NotificationParticipant() { }

	public NotificationParticipant(int notificationId, int participantId)
	{
		NotificationId = notificationId;
		ParticipantId = participantId;
	}
}

