namespace NotificationApp.Domain.Entities;
public sealed class Partner
{
	public int PartnerId { get; private set; }
	public string PartnerName { get; private set; }
	public string? EmailAddress { get; private set; }

	private readonly List<Participant> _participants = new();
	public IReadOnlyCollection<Participant> Participants
		=> _participants.AsReadOnly();

	private readonly List<Notification> _notifications = new();
	public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

	internal Partner() { }

	public Partner(int partnerId, string partnerName, string? emailAddress)
	{
		if (string.IsNullOrWhiteSpace(partnerName))
			throw new ArgumentException("partnerName is required", nameof(partnerName));
		PartnerId = partnerId;
		PartnerName = partnerName;
		EmailAddress = emailAddress;
	}

	public void ChangeName(string newName)
	{
		if (string.IsNullOrWhiteSpace(newName))
			throw new ArgumentException("newName is required", nameof(newName));
		PartnerName = newName;
	}

	public void ChangeEmail(string? newEmail) => EmailAddress = newEmail;

	public void AddParticipant(Participant p)
	{
		if (!_participants.Contains(p))
			_participants.Add(p);
	}
	public void RemoveParticipant(Participant participant)
	{
		_participants.Remove(participant);
	}
	public void AddNotification(Notification notification)
	{
		if (!_notifications.Contains(notification))
			_notifications.Add(notification);
	}

	public void RemoveNotification(Notification notification)
	{
		_notifications.Remove(notification);
	}
}