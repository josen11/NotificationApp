namespace NotificationApp.Domain.Entities;
public sealed class Notification
{
	public int NotificationId { get; private set; }
	public string Title { get; private set; }
	public string Message { get; private set; }
	public string? WebLink { get; private set; }
	public DateTime? ExpiresAtUtc { get; private set; }
	public bool IsHighlighted { get; private set; }
	public int CreatedByOracleEmployeeId { get; private set; }
	public DateTimeOffset CreatedAtUtc { get; private set; }
	public int? UpdatedByOracleEmployeeId { get; private set; }
	public DateTimeOffset? UpdatedAtUtc { get; private set; }
	public int NotificationTypeId { get; private set; }

	private readonly List<Participant> _participants = new();
	public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();

	private readonly List<Partner> _partners = new();
	public IReadOnlyCollection<Partner> Partners => _partners.AsReadOnly();


	internal Notification() { }

	public Notification(int notificationId,
						string title,
						string message,
						string? webLink,
						DateTime? expiresAtUtc,
						bool isHighlighted,
						int createdByOracleEmployeeId,
						DateTimeOffset createdAtUtc,
						int? updatedByOracleEmployeeId,
						DateTimeOffset? updatedAtUtc,
						int notificationTypeId)
	{
		if (string.IsNullOrWhiteSpace(title))
			throw new ArgumentException("title is required", nameof(title));
		if (string.IsNullOrWhiteSpace(message))
			throw new ArgumentException("message is required", nameof(message));

		NotificationId = notificationId;
		Title = title;
		Message = message;
		WebLink = webLink;
		ExpiresAtUtc = expiresAtUtc;
		IsHighlighted = isHighlighted;
		CreatedByOracleEmployeeId = createdByOracleEmployeeId;
		CreatedAtUtc = createdAtUtc;
		UpdatedByOracleEmployeeId = updatedByOracleEmployeeId;
		UpdatedAtUtc = updatedAtUtc;
		NotificationTypeId = notificationTypeId;
	}

	public void ChangeTitle(string newTitle)
	{
		if (string.IsNullOrWhiteSpace(newTitle))
			throw new ArgumentException("newTitle is required", nameof(newTitle));
		Title = newTitle;
	}

	public void ChangeMessage(string newMessage)
	{
		if (string.IsNullOrWhiteSpace(newMessage))
			throw new ArgumentException("newMessage is required", nameof(newMessage));
		Message = newMessage;
	}

	public void ChangeWebLink(string? newLink) => WebLink = newLink;
	public void SetExpiresAtUtc(DateTime? dt) => ExpiresAtUtc = dt;
	public void Highlight() => IsHighlighted = true;
	public void Unhighlight() => IsHighlighted = false;
	public void SetUpdatedMeta(int updatedById, DateTimeOffset updatedAt)
	{
		UpdatedByOracleEmployeeId = updatedById;
		UpdatedAtUtc = updatedAt;
	}
	public void AddParticipant(Participant participant)
	{
		if (!_participants.Contains(participant))
			_participants.Add(participant);
	}

	public void RemoveParticipant(Participant participant)
	{
		_participants.Remove(participant);
	}

	public void AddPartner(Partner partner)
	{
		if (!_partners.Contains(partner))
			_partners.Add(partner);
	}

	public void RemovePartner(Partner partner)
	{
		_partners.Remove(partner);
	}
}
