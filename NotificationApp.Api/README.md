# General config
Create the Solution & Projects
```bash
mkdir NotificationApp
cd NotificationApp

dotnet new sln -n NotificationApp

# Domain: plain C# classes, no external deps
dotnet new classlib -n NotificationApp.Domain
# Application: interfaces, use-cases, DTOs
dotnet new classlib -n NotificationApp.Application
# Infrastructure: EF Core, DB context, repo impls
dotnet new classlib -n NotificationApp.Infrastructure
# API: minimal-API host
dotnet new web -n NotificationApp.Api

# add to solution
dotnet sln add NotificationApp.Domain/NotificationApp.Domain.csproj
dotnet sln add NotificationApp.Application/NotificationApp.Application.csproj
dotnet sln add NotificationApp.Infrastructure/NotificationApp.Infrastructure.csproj
dotnet sln add NotificationApp.Api/NotificationApp.Api.csproj
```

So final structure will be like this:
```bash
NotificationApp
│   NotificationApp.sln
└───NotificationApp.Domain
│       NotificationApp.Domain.csproj
└───NotificationApp.Application
│       NotificationApp.Application.csproj
└───NotificationApp.Infrastructure
│       NotificationApp.Infrastructure.csproj
└───NotificationApp.Api
        NotificationApp.Api.csproj
```

Remember de rules of Clean Architecture:
- The outer layers can depend on the inner layers, but not vice versa.
- Applicaitlayer can depend on Domain layer, but not vice versa.
- Infrastructure layer can depend on Application layer, but not vice versa.
- API layer can depend on Infrastructure layer or Application layer, but not vice versa.

Because .NET Core SDK-style projects bring in transitive references by default, Infrastructure will see Domain’s assemblies (and be able to using NotificationApp.Domain.Entities;) without a direct project reference.
Why this preserves purist Clean Architecture
- Domain (your core business types) has no outward references.
- Application depends on Domain and exposes only the interfaces your use-cases need.
- Infrastructure depends only on Application, implements those interfaces, and (transitively) picks up the Domain types.
- API depends on Application (for the use-case contracts) and on Infrastructure (for the concrete wiring).

That way, the only layer with a compile-time dependency on Domain is Application, exactly as the Dependency Rule dictates.

Db script
```sql
CREATE TABLE [Partner] (
  partnerId         INT PRIMARY KEY,
  partnerName        VARCHAR(300) NOT NULL,
  emailAddress       VARCHAR(255) NULL
);

-- TODO Review with Amy vs Person Table
CREATE TABLE [Participant] (
  participantId      INT PRIMARY KEY,
  firstName          VARCHAR(300) NOT NULL,
  lastName           VARCHAR(300) NOT NULL,
  middleName         VARCHAR(300) NULL,
  isDeceased     BIT NOT NULL,
  isVIP         BIT NOT NULL,
  isEscalated     BIT NOT NULL
);

CREATE TABLE [PartnerParticipantIdentificationMap] (
  partnerId    INT NOT NULL
  CONSTRAINT FK_PartnerParticipant_Partner FOREIGN KEY (partnerId) REFERENCES [Partner](partnerId),
  participantId  INT NOT NULL
  CONSTRAINT FK_PartnerParticipant_Participant FOREIGN KEY (participantId) REFERENCES [Participant](participantId),
  PRIMARY KEY (partnerId, participantId)
);

-- TODO: Ask to AMy about ParentCampaignId
-- TODO: Ask to Amy A Partner only have 1 Campaign
-- TODO: Ask if campaing to individuals (Shopper)
--CREATE TABLE [Campaign] (
--  campaignId         INT PRIMARY KEY,
--  campaignName     VARCHAR(300) NOT NULL,
--  parentCampaignId   INT NULL
--  CONSTRAINT FK_Campaign_Campaign FOREIGN KEY([parentCampaignId]) REFERENCES [Campaign] ([campaignId]),
--  partnertId         INT NOT NULL
--    CONSTRAINT FK_Campaign_Partner FOREIGN KEY (partnertId) REFERENCES [Partner](partnertId)
--);

CREATE TABLE NotificationType (
  notificationTypeId INT IDENTITY PRIMARY KEY,
  typeName VARCHAR(50) NOT NULL UNIQUE
);

-- Notification master table
CREATE TABLE [Notification] 
(
  notificationId             INT IDENTITY(1,1) PRIMARY KEY,
  title                      VARCHAR(300) NOT NULL,
  message                    VARCHAR(1000) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NOT NULL,
  webLink                    VARCHAR(500) NULL,
  expiresAtUTC               DATETIME2(2),
  isHighlighted              BIT NULL DEFAULT(0),
  createdByOracleEmployeeId  INT NOT NULL,
  createdAtUTC               DATETIMEOFFSET(2) NOT NULL,
  updatedByOracleEmployeeId  INT NULL,
  updatedAtUTC               DATETIMEOFFSET(2) NULL,
  notificationTypeId     INT FOREIGN KEY REFERENCES NotificationType(notificationTypeId)
);

CREATE TABLE [NotificationParticipant] (
  notificationId    INT NOT NULL
  CONSTRAINT FK_NotificationParticipant_Notification FOREIGN KEY (notificationId) REFERENCES [Notification](notificationId),
  participantId     INT NOT NULL
  CONSTRAINT FK_NotificationParticipant_Participant FOREIGN KEY (participantId) REFERENCES [Participant](participantId),
  PRIMARY KEY (notificationId, participantId)
);

CREATE TABLE [NotificationPartner] (
  notificationId    INT NOT NULL
  CONSTRAINT FK_NotificationPartner_Notification FOREIGN KEY (notificationId) REFERENCES [Notification](notificationId),
  partnerId          INT NOT NULL
  CONSTRAINT FK_NotificationPartner_Partner FOREIGN KEY (PartnerId) REFERENCES [Partner](partnerId),
  PRIMARY KEY (notificationId, partnerId)
);

--CREATE TABLE [NotificationCampaign] (
--  notificationId    INT NOT NULL
--  CONSTRAINT FK_NotificationCampaign_Notification FOREIGN KEY (notificationId) REFERENCES [Notification](notificationId),
--  campaignId        INT NOT NULL
--  CONSTRAINT FK_NotificationCampaign_Campaign FOREIGN KEY (CampaignId) REFERENCES [Campaign](campaignId),
--  PRIMARY KEY (notificationId, campaignId)
--);

Insert into NotificationType values ('Test')
```

### Scaffold Your Database into Infrastructure Layer
We can follow the individual approach by db objects, or we can scaffold the entire database. In this case, we will scaffold only the NotificationType table.

To avoid this error, You can add `TrustServerCertificate=True` to your connection string:
| A connection was successfully established with the server, but then an error occurred during the login process. (provider: SSL Provider, error: 0 - The certificate chain was issued by an authority that is not trusted.)| 
```powershell
cd NotificationApp.Infrastructure

dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Scaffold only NotificationType (no pluralization, keep names as in DB)
dotnet ef dbcontext scaffold `
    "Server=.;Database=LocalServiceCenter;Trusted_Connection=True;TrustServerCertificate=True;" `
    Microsoft.EntityFrameworkCore.SqlServer `
    --output-dir Data/Models `
    --context-dir Data `
    --context NotificationDbContext `
    --schema dbo `
    --table NotificationType `
    --no-pluralize `
    --use-database-names
```

Scaffolding your entire database instead of just the tables you’re working with is certainly possible—and it’ll give you models and DbSets for every table in one go—but it comes with trade-offs:
- Model Clutter: You’ll end up with dozens (or hundreds) of EF model classes and DbSet properties you may never use. That makes your Infrastructure project harder to navigate and inflates your CI build times.
- Tighter Coupling to Schema: Full-DB scaffolding couples your codebase to every detail of your database’s schema—even tables that aren’t part of your current domain. Any schema change anywhere forces you to reconcile all those generated classes.
- Domain-First Intent: With a purist Clean/DDD approach you want the domain to dictate what gets modeled, not the other way around. By scaffolding only the aggregates you care about (e.g. NotificationType), you keep your Infrastructure lean and your Domain mappings intentional.

In this case our db dont have a lot DB so as first approach we can scaffold the entire DB. In this case, we will scaffold only the NotificationType table.
```powershell
cd NotificationApp.Infrastructure

dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Scaffold all db
dotnet ef dbcontext scaffold `
    "Server=.;Database=LocalServiceCenter;Trusted_Connection=True;TrustServerCertificate=True;" `
    Microsoft.EntityFrameworkCore.SqlServer `
    --output-dir Data/Models `
    --context-dir Data `
    --context NotificationDbContext `
    --no-pluralize `
    --use-database-names
```

This generates:
- Data/NotificationDbContext.cs
- Data/Models/NotificationType.cs (the EF-generated model)

## “Formula” for handling future schema changes
Whenever the database schema changes, follow these steps:
- (Optional) Backup your existing Data/Models folder (just in case).
- Re‐scaffold your models—either all tables or only the ones that changed:

All tables (overwrite everything)

```powershell
dotnet ef dbcontext scaffold `
  "<your-conn-string>" `
  Microsoft.EntityFrameworkCore.SqlServer `
  --output-dir Data/Models `
  --context-dir Data `
  --context NotificationDbContext `
  --no-pluralize `
  --use-database-names `
  --force
```
Only specific tables (e.g. new or altered ones)

```powershell
dotnet ef dbcontext scaffold `
  "<conn>" `
  Microsoft.EntityFrameworkCore.SqlServer `
  --output-dir Data/Models `
  --context-dir Data `
  --context NotificationDbContext `
  --no-pluralize `
  --use-database-names `
  --table NotificationType `
  --table Notification
```

- Review the diffs in your version control. EF scaffolding may update nullability, column names, or drop/add classes.
- Merge in your manual tweaks (e.g. nullability corrections, base‐class adjustments).
- Update your mapping extensions—for each new or changed model you’ll need a ToDomain() / ToModel() mapping.
- Add or adjust repositories and DI registrations for any new aggregates.
- Rebuild & run tests (or smoke-test your API) to catch any mapping or contract breakages.

### When to scaffold both tables
- Schema change on A only, but relationship still intact:
  - You can scaffold just A (using --table A) if:
  - B’s model already exists and you haven’t modified its structure.
  - You don’t need to update B.

- Schema change on A and/or B that affects their relationship:
  - You must include both tables in your scaffold command so EF can:
  - Regenerate A’s model (with updated FK/nav-prop).
  - Regenerate B’s model (so the CLR type matches the database schema).

Example:
```powershell
dotnet ef dbcontext scaffold `
  "<your-conn-string>" `
  Microsoft.EntityFrameworkCore.SqlServer `
  --output-dir Data/Models `
  --context-dir Data `
  --context NotificationDbContext `
  --no-pluralize `
  --use-database-names `
  --table NotificationType `
  --table Notification `
  --force
```
This ensures both NotificationType and Notification classes are up-to-date, and their relationship (navigation properties, FK annotations) is scaffolded correctly.


## Why this works
- You keep your domain and application layers stable—only Infrastructure regenerates.
- By scoping the scaffold to changed tables, you minimize churn.
- By reviewing and merging diffs, you retain control over naming, nullable references, and any hand-crafted behaviors.

That “scaffold → review → merge → update mappings → test” cycle is your go-to formula any time the DB changes.

## Mapping between EF Model and Domain with extension methods
- We are using extension methods and alias. We are mapping these and also scalffolding the database. We are not using AutoMapper because we want to keep it simple and avoid adding a dependency.

```csharp
using DomainPartner = NotificationApp.Domain.Entities.Partner;
using ModelPartner = NotificationApp.Infrastructure.Data.Models.Partner;
```
Going with hand-crafted extension-method mappers instead of a magic mapper library (AutoMapper/Mapperly) gives you:
- Full Control & Explicitness:
  - Visibility: Every property mapping lives in one place, in plain C#. You never wonder “what did the profile do?”
  - No Surprises: If you rename a field, the compiler will flag the missing mapping. With AutoMapper, you can get runtime errors or silently skipped fields if your conventions drift.
- Zero “Magic” & Predictable Performance: 
  - No Reflection or Code Generation at Runtime: Extension methods are just plain methods—no startup scanning, no IL generation, no hidden allocations. It’s as fast as calling any other method.
  - Easy to Benchmark/Tune: You can see exactly what SQL or LINQ is being produced, and there’s no intermediate expression tree or projection surprises.
- Domain Integrity & Invariants
  - Constructor Enforcements: You must call your domain constructors, which run all your invariant checks (e.g. non-empty names). AutoMapper can bypass constructors or setters unless you explicitly configure it.
  - Guard Rails: By mapping through your public domain ctor/methods you guarantee your aggregate invariants are always applied.
- Lower Dependency Footprint
  - No Extra NuGet: You don’t pull in AutoMapper (or its configuration DSL) and you avoid updating it when new versions come out.
  - No Learning Curve: New developers don’t have to learn AutoMapper profiles, conventions, or the quirks of Mapperly’s source-generation attributes. They just read a static method.
- Easier to Debug & Trace
  - Step-through: You can set a breakpoint in the extension method. If something’s wrong, you see exactly which value is wrong and why.
  - Compiler-assisted: If you add a new property to your domain, the compiler will remind you to add it to the mapping.

## When to Consider AutoMapper/Mapperly
That said, there are scenarios where a mapping library might save you boilerplate:
- Large DTO Surface: If you have hundreds of very similar DTOs and you’re doing 1:1 property copies with no logic, AutoMapper profiles or Mapperly source-gen can reduce the typing.
- Project-wide Conventions: If you really lean on naming conventions (e.g. strip “Dto” suffix everywhere) and you don’t anticipate complicated ctor logic.

But for a DDD/Clean-Architecture setup where domain constructors enforce invariants and mapping logic may evolve (e.g. default values, nested aggregates), explicit extension methods are often the clearer, safer, and higher-performance choice.

## EF Core does not generate Junction table
EF Core’s reverse-engineering will happily scaffold most tables, but by default it treats a “pure” many-to-many join table (i.e. a table whose only columns are two FKs + a composite PK) as a skip-navigation rather than as its own entity class. That means:
- No PartnerParticipantIdentificationMap class is generated.
- Instead, your two principal models (Partner and Participant) get many-to-many navigations (e.g. ICollection<Participant> Partners on Partner, and vice-versa).

Why it does this
- Starting in EF Core 5, many-to-many without payload is a first-class feature. If a table has no extra data beyond the FKs, EF assumes you want an implicit join and omits the scaffolded CLR type.

How to get an explicit entity
- Add a “payload” column (even something like a CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()) to that table—then EF treats it as a full entity and will scaffold it.
- Manually define the model yourself in Infrastructure/Data/Models (you’ve already hand-created the mapping extensions, so you could just add the class there).
- Use EF Core Power Tools (a VS extension) which can be configured to always generate join-entity classes even for pure join tables.

If you didn’t include it in your scaffold command
- Make sure your dotnet ef dbcontext scaffold invocation explicitly lists that table:

```powershell
--table PartnerParticipantIdentificationMap
```
Without --table EF will by default grab all tables—but then still skip the pure join table as a separate class as described above.

In short, the missing model is intentional: EF Core is giving you built-in many-to-many support instead of forcing you to work with the join class. If you really need that CLR type, add some non-FK column (payload) or scaffold it manually.

## Mapping Junction Table to Domain
You actually have two very viable paths here:

### 1. Treat it as a “pure” many-to-many (no CLR type)
Since your join‐table has no payload, EF Core 5+ will, by convention, skip generating a PartnerParticipantIdentificationMap class and instead wire up your two principal entities:

```csharp
// In Partner entity (Domain) you’d add:
private readonly List<Participant> _participants = new();
public IReadOnlyCollection<Participant> Participants 
    => _participants.AsReadOnly();

public void AddParticipant(Participant p)
{
  if (!_participants.Contains(p))
    _participants.Add(p);
}

// Similarly, in Participant:
private readonly List<Partner> _partners = new();
public IReadOnlyCollection<Partner> Partners 
    => _partners.AsReadOnly();

public void AddPartner(Partner p)
{
  if (!_partners.Contains(p))
    _partners.Add(p);
}
```

And in your DbContext.OnModelCreating:

```csharp
modelBuilder.Entity<Partner>()
  .HasMany(p => p.Participants)
  .WithMany(p => p.Partners)
  .UsingEntity("PartnerParticipantIdentificationMap",
     j => j.HasOne<Participant>()
           .WithMany()
           .HasForeignKey("participantId"),
     j => j.HasOne<Partner>()
           .WithMany()
           .HasForeignKey("partnerId"));
```
No separate mapping extensions are needed for the join table.

Your API can expose e.g.

```bash
POST /partners/{id}/participants/{pid}
```
which under the covers just calls partner.AddParticipant(participant) + SaveChangesAsync().

### 2. Keep it as an explicit entity
If you’d rather have a first-class CLR type (because you want to treat the association as an aggregate root in its own right, or you plan on adding audit columns later), you can:
- Manually re-add the PartnerParticipantIdentificationMap model class (or add a trivial CreatedAtUtc column so scaffolding will generate it).
- Keep your ModelMappingExtensions and repository for it—exactly as you did for the other tables.
- Expose its own endpoints if you need them, or simply use the map-repository inside a domain service that manages the association.

You do not need a payload column today—your manual mapping extension is enough. But you will have to:
- Scaffold it manually via --table PartnerParticipantIdentificationMap and
- Make sure you add those two alias usings so ModelPartnerParticipantMap and DomainPartnerParticipantMap aren’t ambiguous.

### Which is “better”?
- If you never need extra data on the join (timestamps, flags, etc.), the skip-nav approach (Option 1) is more concise and leans into EF Core’s conventions.
- If you foresee evolving that relationship into its own aggregate—with business rules on when a Partner↔Participant link can be created or destroyed—then Option 2 (explicit entity) gives you full control from day one.
Either way, you don’t have to stick a payload column in the table unless you actually need one.


## Env variables appsettings.json
In ASP NET Core the recommended pattern is to keep your secrets and environment‐specific values out of source‐controlled JSON and instead let environment variables override your JSON settings. Here’s how you wire it all up:
### 1. Define your placeholder in appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""    // ← leave blank, we’ll fill via env var
  },
  "JwtSettings": {
    "Issuer":    "",           // ← same here
    "Audience":  "",
    "SecretKey": ""
  }
}
```
By supplying empty strings (or reasonable defaults) here you document the shape of your config without committing real secrets.

### 2. For local development, put the actual values in launchSettings.json
In Properties/launchSettings.json:

```json

{
  "profiles": {
    "NotificationApp.Api": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT":             "Development",
        "ConnectionStrings__DefaultConnection": 
          "Server=(localdb)\\mssqllocaldb;Database=NotificationDb;Trusted_Connection=True;MultipleActiveResultSets=true",
        "JwtSettings__Issuer":                "MyDevIssuer",
        "JwtSettings__Audience":              "MyDevAudience",
        "JwtSettings__SecretKey":             "super-secret-dev-key"
      }
    }
  }
}
```
Notice the naming convention:
 - Double-underscore (__) to descend into JSON objects
 - ConnectionStrings__DefaultConnection → Configuration["ConnectionStrings:DefaultConnection"]

### 3. In Program.cs, make sure you add the environment-variables provider
The default ASP NET Core template already does this for you, but if you’ve customized things:

```csharp
var builder = WebApplication.CreateBuilder(args);

// ensure env vars override JSON
builder.Configuration
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddJsonFile(
           $"appsettings.{builder.Environment.EnvironmentName}.json",
           optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

// register your DbContext
builder.Services.AddDbContext<NotificationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 4. On your production server / container
You do not check secrets into Git. Instead you set real environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=prod;Database=ProdDb;User Id=prod;Password=...;"
export JwtSettings__Issuer="MyProdIssuer"
export JwtSettings__Audience="MyProdAudience"
export JwtSettings__SecretKey="very-secure-key"
```
or in Kubernetes:

```yaml
env:
  - name: ConnectionStrings__DefaultConnection
    valueFrom:
      secretKeyRef:
        name: prod-sql-secret
        key: connectionString
  - name: JwtSettings__SecretKey
    valueFrom:
      secretKeyRef:
        name: jwt-secret
        key: secretKey
```
### Why this approach?
- No secrets in source control
- Automatic override: environment variables trump JSON values
- Same code works locally (via launchSettings.json) and in production
- Strong typing in your Options<T> bindings—e.g. builder.Services.Configure<JwtSettings>(...)—lets you inject strongly-typed config objects throughout your app.

## Swagger
To get Swagger up and running you only need to pull in Swashbuckle’s NuGet package (the AddEndpointsApiExplorer call is in-box on .NET 8, so no extra package there):

```bash
cd NotificationApp.Api
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```
That brings in:
- Swashbuckle.AspNetCore.SwaggerGen (for AddSwaggerGen())
- Swashbuckle.AspNetCore.SwaggerUI (for UseSwaggerUI())

Once that’s installed, your calls to builder.Services.AddEndpointsApiExplorer() and builder.Services.AddSwaggerGen()—and the app.UseSwagger() + app.UseSwaggerUI() in Program.cs—will compile and serve the interactive UI.