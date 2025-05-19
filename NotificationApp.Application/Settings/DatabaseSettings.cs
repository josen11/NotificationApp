namespace NotificationApp.Application.Settings;
public sealed class DatabaseSettings
{
	public string DefaultConnection { get; set; } = null!;
	public int CommandTimeoutSeconds { get; set; } = 30;
}