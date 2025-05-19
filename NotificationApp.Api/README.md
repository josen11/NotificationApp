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
```powershell
cd NotificationApp.Infrastructure

dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Scaffold only NotificationType (no pluralization, keep names as in DB)
dotnet ef dbcontext scaffold `
    "Server=.;Database=LocalServiceCenter;Trusted_Connection=True;" `
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

# Scaffold only NotificationType (no pluralization, keep names as in DB)
dotnet ef dbcontext scaffold `
    "Server=.;Database=LocalServiceCenter;Trusted_Connection=True;" `
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