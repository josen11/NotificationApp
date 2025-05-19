using FluentValidation;
using Microsoft.Extensions.Options;

namespace NotificationApp.Api.Chassis.Validations;

public class FluentValidateOptions<TOptions> : IValidateOptions<TOptions>
	where TOptions : class
{
	private readonly IServiceProvider _sp;
	private readonly string? _name;
	public FluentValidateOptions(IServiceProvider sp, string? name)
	{
		_sp = sp;
		_name = name;
	}

	public ValidateOptionsResult Validate(string? name, TOptions options)
	{
		if (_name is not null && _name != name)
			return ValidateOptionsResult.Skip;

		var validator = _sp.CreateScope()
			.ServiceProvider
			.GetRequiredService<IValidator<TOptions>>();

		var result = validator.Validate(options);
		if (result.IsValid)
			return ValidateOptionsResult.Success;

		var errors = result.Errors
			.Select(f => $"{typeof(TOptions).Name}.{f.PropertyName}: {f.ErrorMessage}")
			.ToArray();
		return ValidateOptionsResult.Fail(errors);
	}
}

public static class OptionsBuilderExtensions
{
	public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
		this OptionsBuilder<TOptions> builder)
		where TOptions : class
	{
		// wire up the custom IValidateOptions<TOptions>
		builder.Services.AddSingleton<IValidateOptions<TOptions>>(sp =>
			new FluentValidateOptions<TOptions>(sp, builder.Name));
		return builder;
	}
}