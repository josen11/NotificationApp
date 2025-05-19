namespace NotificationApp.Domain.Entities;
public sealed class NotificationPartner
{
	public int NotificationId { get; private set; }
	public int PartnerId { get; private set; }

	internal NotificationPartner() { }

	public NotificationPartner(int notificationId, int partnerId)
	{
		NotificationId = notificationId;
		PartnerId = partnerId;
	}
}
