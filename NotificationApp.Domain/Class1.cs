namespace NotificationApp.Domain;

public sealed class NotificationType
{
	public int Id { get; private set; }
	public string Name { get; private set; }

	// EF Core needs a parameterless ctor (internal to prevent misuse)
	internal NotificationType() { }

	public NotificationType(int id, string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name required", nameof(name));

		Id = id;
		Name = name;
	}

	public void ChangeName(string newName)
	{
		if (string.IsNullOrWhiteSpace(newName))
			throw new ArgumentException("Name required", nameof(newName));
		Name = newName;
	}
}