using FluentValidation;
using NotificationApp.Application.Settings;

namespace NotificationApp.Application.Validators;
public class DatabaseSettingsValidator
	: AbstractValidator<DatabaseSettings>
{
	public DatabaseSettingsValidator()
	{
		RuleFor(x => x.DefaultConnection)
			.NotEmpty().WithMessage("Connection string must be provided");
		RuleFor(x => x.CommandTimeoutSeconds)
			.GreaterThan(0).WithMessage("Timeout must be positive");
	}
}
