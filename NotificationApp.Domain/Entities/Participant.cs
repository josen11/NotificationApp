namespace NotificationApp.Domain.Entities;
public sealed class Participant
{
	public int ParticipantId { get; private set; }
	public string FirstName { get; private set; }
	public string LastName { get; private set; }
	public string? MiddleName { get; private set; }
	public bool IsDeceased { get; private set; }
	public bool IsVip { get; private set; }
	public bool IsEscalated { get; private set; }

	private readonly List<Partner> _partners = new();

	public IReadOnlyCollection<Partner> Partners
		=> _partners.AsReadOnly();

	private readonly List<Notification> _notifications = new();
	public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

	internal Participant() { }

	public Participant(int participantId, string firstName, string lastName, string? middleName,
		bool isDeceased, bool isVip, bool isEscalated)
	{
		if (string.IsNullOrWhiteSpace(firstName))
			throw new ArgumentException("firstName is required", nameof(firstName));
		if (string.IsNullOrWhiteSpace(lastName))
			throw new ArgumentException("lastName is required", nameof(lastName));

		ParticipantId = participantId;
		FirstName = firstName;
		LastName = lastName;
		MiddleName = middleName;
		IsDeceased = isDeceased;
		IsVip = isVip;
		IsEscalated = isEscalated;
	}

	public void ChangeFirstName(string newFirst)
	{
		if (string.IsNullOrWhiteSpace(newFirst))
			throw new ArgumentException("newFirst is required", nameof(newFirst));
		FirstName = newFirst;
	}

	public void ChangeLastName(string newLast)
	{
		if (string.IsNullOrWhiteSpace(newLast))
			throw new ArgumentException("newLast is required", nameof(newLast));
		LastName = newLast;
	}

	public void ChangeMiddleName(string? newMiddle) => MiddleName = newMiddle;
	public void MarkDeceased() => IsDeceased = true;
	public void MarkVip() => IsVip = true;
	public void MarkEscalated() => IsEscalated = true;
	public void AddPartner(Partner p)
	{
		if (!_partners.Contains(p))
			_partners.Add(p);
	}
	public void RemovePartner(Partner partner)
	{
		_partners.Remove(partner);
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

