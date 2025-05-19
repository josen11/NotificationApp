using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NotificationApp.Api.Features;
using NotificationApp.Application.Settings;
using NotificationApp.Application.Validators;
using NotificationApp.Infrastructure;
using NotificationApp.Infrastructure.Data;
using Microsoft.Extensions.Options;
using NotificationApp.Api.Chassis.Validations;

var builder = WebApplication.CreateBuilder(args);

// Load JSON + env-vars
builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile(
		$"appsettings.{builder.Environment.EnvironmentName}.json",
		optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();

// Register your FluentValidation validator
builder.Services
	.AddTransient<IValidator<DatabaseSettings>, DatabaseSettingsValidator>();

// Bind & validate your DatabaseSettings via the Options pattern
builder.Services
	.AddOptions<DatabaseSettings>()
	.Bind(builder.Configuration.GetSection("ConnectionStrings"))
	// Option 1
	//.Validate(settings =>
	//{
	//	var validator = new DatabaseSettingsValidator();
	//	var result = validator.Validate(settings);
	//	return result.IsValid;
	//}, "DatabaseSettings validation failed")
	// Option 2
	.ValidateFluentValidation()
	.ValidateOnStart();              // throws if invalid on startup

// Add services for Minimal-API & Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Old approach to validate options
// Register your DbContext (Infrastructure) via the connection string
//var connString = builder.Configuration.GetConnectionString("DefaultConnection");
//if (string.IsNullOrWhiteSpace(connString))
//{
//	throw new InvalidOperationException(
//		"ConnectionStrings:DefaultConnection is not configured!");
//}

// Use the connection string from the DatabaseSettings section
builder.Services.AddDbContext<NotificationDbContext>((sp, opts) =>
{
	var dbSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
	opts.UseSqlServer(dbSettings.DefaultConnection, sql =>
		sql.CommandTimeout(dbSettings.CommandTimeoutSeconds));
	//opts.UseSqlServer(connString));
});

// Register all repositories & DbContext via your Infrastructure DI helper
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
// Enable Swagger in Development (you can remove the env check if you want it in Prod too)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API V1");
		c.RoutePrefix = ""; // serve UI at root (/)
	});
}

//  Map endpoint groups

app.MapPartnerEndpoints();
app.MapParticipantEndpoints();
app.MapNotificationTypeEndpoints();
app.MapNotificationEndpoints();

app.Run();
