# Options Pattern with fluent validation
Using the Options Pattern to centralize and validate configuration buys you:
 - Type safety (no more magic strings scattered through Program.cs)
 - Centralized validation/early‐fail on startup if something’s mis-configured
 - Decoupling so any part of your app can request a fully-populated settings object

That said, for a single connection string that only your Infrastructure layer uses, the built-in

```csharp
builder.Services.AddDbContext<NotificationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```
is succinct and fine. If you know you’ll never need that setting elsewhere, you don’t strictly have to introduce an extra settings class.

## 1. When to keep it simple (current approach)
```csharp
// Program.cs
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NotificationDbContext>(opts =>
    opts.UseSqlServer(conn));
```
- Pros: zero ceremony, direct, everyone reading Program.cs instantly sees where the DB comes from.
- Cons: no validation, no easy way to inject that same setting into other services (e.g. background jobs), and if you typo the key you get a runtime‐only error inside EF.

## 2. When to introduce Options Pattern + Validation
If you anticipate:
- More related settings (e.g. command‐timeout, pool sizes, audit flags… all under a “Database” umbrella)
- Needing to inject the connection string (or related DB flags) into other services
- Or simply wanting compile‐time “contracts” for your configuration

### Why FluentValidation Over Data Annotations?
Data Annotations work well for simple validations, but FluentValidation offers several advantages:
- More expressive and flexible validation rules
- Better support for complex conditional validations
- Cleaner separation of concerns (validation logic separate from model)
- Easier testing of validation rules
- Better support for custom validation logic
- Allows for injecting dependencies into validators

### Understanding the Options Pattern Lifecycle
Before diving deep into validation, it's important to understand the lifecycle of options in ASP.NET Core:
- Options are registered with the DI container
- Configuration values are bound to options classes
- Validation occurs (if configured)
- Options are resolved when requested via IOptions<T>, IOptionsSnapshot<T>, or IOptionsMonitor<T>
The ValidateOnStart() method forces validation to occur during application startup rather than when options are first resolved.

### Install FluentValidation in your Application project
```bash
cd NotificationApp.Application
dotnet add package FluentValidation
```
### Create your POCO

```csharp
// Application/Settings/DatabaseSettings.cs
namespace NotificationApp.Application.Settings
{
    public class DatabaseSettings
    {
        public string DefaultConnection { get; set; } = null!;
        public int CommandTimeoutSeconds { get; set; } = 30;
    }
}
```

###  Create the FluentValidation validator:

```csharp
// Application/Validators/DatabaseSettingsValidator.cs
using FluentValidation;
using NotificationApp.Application.Settings;

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
```

### Wire it up in Program.cs
We have 2 ways we create our own 

| Note: .ValidateFluentValidation() from FluentValidation.DependencyInjectionExtensions Nuget Package is not working for that reason we present 2 options here:

Option 1: Manual options validation

```csharp
using FluentValidation;
using FluentValidation.DependencyInjectionExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationApp.Application.Settings;
using NotificationApp.Infrastructure;
using NotificationApp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) Load JSON + env-vars as before…
builder.Configuration
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
  .AddEnvironmentVariables();

// 2) Register your validator & enable FluentValidation
builder.Services
    .AddTransient<IValidator<DatabaseSettings>, DatabaseSettingsValidator>();

// 3) Bind & validate your DatabaseSettings via the Options pattern
builder.Services
    .AddOptions<DatabaseSettings>()
    .Bind(builder.Configuration.GetSection("ConnectionStrings"))  // binds DefaultConnection + any other props
    .Validate(settings =>
  {   var validator = new DatabaseSettingsValidator();
      var result = validator.Validate(settings);
      return result.IsValid;
  }, "DatabaseSettings validation failed")
    .ValidateOnStart();                // throw if invalid at startup

// 4) use them to configure EF
builder.Services.AddDbContext<NotificationDbContext>((sp, opts) =>
{
    var dbSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    opts.UseSqlServer(dbSettings.DefaultConnection, sql =>
        sql.CommandTimeout(dbSettings.CommandTimeoutSeconds));
});

// 5) register your repositories etc.
builder.Services.AddInfrastructure(builder.Configuration);

...
```

Option 2: Manual options validation
Implement this extension method in your API project Chassis folder :
```csharp
using Microsoft.Extensions.Options;
using FluentValidation;
using System;

namespace NotificationApp.Infrastructure.Options
{
  // this is straight from the blog
  public class FluentValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
  {
    private readonly IServiceProvider _sp;
    private readonly string? _name;
    public FluentValidateOptions(IServiceProvider sp, string? name)
    {
      _sp   = sp;
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
}
```

Then in your Program.cs, add this line:
```csharp
builder.Services
  .AddOptions<DatabaseSettings>()
  .Bind(…)
  .ValidateFluentValidation()   // ← now comes from your own OptionsBuilderExtensions
  .ValidateOnStart();
```

- Now if your appsettings.json or ConnectionStrings__DefaultConnection env-var is missing or empty, startup will blow up with a clear validation error.
- You’ve also decoupled “where‐does-my-DB‐go” from EF and can pass those settings into any other service that needs them.

### 3. Leaving the local connection string in appsettings.json
It’s perfectly fine to keep:
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NotificationDb;Trusted_Connection=True;"
  }
}
```
for your developer machine. At runtime you can still override via:
- ConnectionStrings__DefaultConnection env-var on your CI/CD agent or production host
- An appsettings.Production.json that’s not checked into Git

This gives you:
- Minimal friction locally (you “just clone & run”)
- Secure overrides in higher environments

## Use the Options pattern in another layers
Because your DatabaseSettings POCO lives in the Application layer, your Infrastructure project already has a reference to it (we wired up NotificationApp.Infrastructure → NotificationApp.Application). From there, any class in Infrastructure can simply take an IOptions<DatabaseSettings> (or even the concrete DatabaseSettings) via constructor DI.
### 1. Register the settings once in Program.cs
You’ve already got:

```csharp
builder.Services
  .AddOptions<DatabaseSettings>()
  .Bind(builder.Configuration.GetSection("ConnectionStrings"))
  .ValidateFluentValidation()
  .ValidateOnStart();
```
That makes

```csharp
IOptions<DatabaseSettings>
IOptionsMonitor<DatabaseSettings>
DatabaseSettings  // if you resolve the Value directly
```
all available from the container.

### 2. Consume it in Infrastructure
a) In your DbContext factory
You did this:

```csharp
builder.Services.AddDbContext<NotificationDbContext>((sp, opts) =>
{
    var dbSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    opts.UseSqlServer(
      dbSettings.DefaultConnection,
      sql => sql.CommandTimeout(dbSettings.CommandTimeoutSeconds));
});
```

That’s one way: the AddDbContext callback resolves IOptions<DatabaseSettings> so the context is configured from the same validated settings.

b) In any other Infra service or repository
If you had, say, a custom service in Infrastructure that needed the raw connection string or the timeout, just inject IOptions<DatabaseSettings>:

```csharp
namespace NotificationApp.Infrastructure.SomeService;

using Microsoft.Extensions.Options;
using NotificationApp.Application.Settings;

public class MyInfraService
{
    private readonly string _conn;
    private readonly int _timeout;

    public MyInfraService(IOptions<DatabaseSettings> dbOptions)
    {
        var s = dbOptions.Value;
        _conn    = s.DefaultConnection;
        _timeout = s.CommandTimeoutSeconds;
    }

    public void DoSomething()
    {
        // use _conn/_timeout here
    }
}
```
Then register that service:

```csharp
builder.Services.AddScoped<MyInfraService>();
```
3. Why this works with Clean Architecture
- Settings live in Application (the inner layer) as a contract.
- Infrastructure (an outer layer) references that contract and uses DI to pull in the validated values—no JSON or env-vars code in Infrastructure.
- No circular deps: only Application → Domain, Infrastructure → Application.

### TL;DR
- Define your POCO in Application.
- Register it via AddOptions<T>() in Program.cs.
- Inject IOptions<DatabaseSettings> anywhere in Infrastructure—DbContext, repositories, custom services—to consume the same, validated configuration.


## Final Recommendation
- If your only config is one connection string, and you don’t need it elsewhere, keep the simple GetConnectionString(...) approach.
- If you foresee more DB-related flags or wider use, adopt the Options Pattern + FluentValidation as shown above.

Both are valid—choose the right balance of simplicity vs. structure for your team.

## Why add Settings classes in Application layer rather than in infrastructure?
Putting your settings classes in the Application layer rather than in Infrastructure follows the same dependency-direction rules that Clean Architecture prescribes:
- Define a “contract” inwards
  - Application is the boundary between your UI/API and your infrastructure. Anything your application “needs” to do its job—whether that’s connection strings, JWT options, feature flags, etc.—is part of its contract.
  - By placing your DatabaseSettings, JwtSettings, or any other XxxSettings POCOs in Application, you’re saying: “These are the configuration values our use-cases depend on.”
- Keep Infrastructure “behind” that contract
  - Infrastructure is allowed to reference Application, but not vice versa. When you bind IOptions<DatabaseSettings> in Program.cs (or in Infrastructure/DependencyInjection), you’re satisfying the Application contract.
  - If you put your settings classes in Infrastructure, you’d introduce a circular dependency or force your Application layer to know about Infrastructure assemblies.
- Testability & Purity
  - Services in your Application layer can depend on IOptions<JwtSettings> or IOptions<DatabaseSettings> without dragging in any concrete Configuration or EF-Core packages. You can write unit tests against your handlers by simply passing in a stubbed IOptions<T>.
  - If those settings lived in Infrastructure, your Application tests would need to pull in Infrastructure or Microsoft.Extensions.Configuration just to construct a settings object.
Separation of Concerns
 - Application classes declare what configuration they need (via constructor injection of IOptions<T>).
 - Infrastructure is responsible for how those options get bound (e.g. AddOptions<DatabaseSettings>().Bind(...)).

In Practice
- Application/Settings/DatabaseSettings.cs: Pure POCO, zero dependencies beyond System.
- Infrastructure/DependencyInjection.cs:  Knows how to bind that POCO to your JSON, environment variables, or vault.

This keeps your Application layer agnostic of where those values come from, and your Infrastructure layer focused on how to wire them up.

## POCO
POCO stands for Plain Old CLR Object. It’s a play on the term “POJO” (“Plain Old Java Object”) from the Java world.
In .NET, a POCO is simply:
- A class or struct
- That doesn’t inherit from any framework-specific base class
- And doesn’t implement any framework-specific interfaces
It’s just your own “plain” type, with only the properties and methods you declare.


### More details
https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers
https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-configuration-provider
https://learn.microsoft.com/en-us/dotnet/core/extensions/options
https://learn.microsoft.com/en-us/dotnet/core/extensions/options-validation-generator
https://learn.microsoft.com/en-us/dotnet/core/extensions/options-library-authors
https://www.milanjovanovic.tech/blog/options-pattern-validation-in-aspnetcore-with-fluentvalidation