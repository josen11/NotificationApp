# Repository Pattern to Presentation Layer Approach
Here’s an end-to-end, step-by-step approach to a “purist” Clean Architecture using EF Core Database-First, .NET 8 and EF Core 9, with a generic repository and minimal-API CRUD for NotificationType. You’ll scaffold your database into the Infrastructure layer, keep your Domain entities untouched, and map between them in repositories.

## Domain layer
### 1. Sealed Classes
- Prevents Uncontrolled Inheritance: By marking a domain entity sealed, you make it impossible for consumers or other layers to create unanticipated subclasses. This ensures that all variations of “what a NotificationType is” live in one place—your domain assembly—so you don’t accidentally violate invariants by someone extending the class elsewhere.
- Clear Intent & Simpler Maintenance: Sealed types express “this is the complete definition.” When future developers see sealed class NotificationType, they know: “There are no other specialized versions hiding out there.” It reduces mental overhead when reasoning about your model.
- Potential Performance Gains: In some runtimes (and with certain JIT optimizations), sealed classes can be slightly faster to dispatch, because the runtime knows there won’t be further subclasses. While usually minor, it’s a nice side benefit.

### 2. Private Setters
- Encapsulation of State: If you expose `public int Id { get; set; }` or `public string Name { get; set; }`, any consumer can change your entity’s key or name at will—possibly violating uniqueness, required-field, or other business rules. By using private set, you ensure that the only ways to change state are:
  - The constructor (initial creation)
  - Explicit methods you provide
- Maintaining Invariants: With private set you write methods like ChangeName(string newName) that can check for non-empty names, max length, or custom validation. You’ll never end up with an entity in an invalid state (“empty name” or “null name”) because you control every mutation point.

### 3. Putting Methods in the Domain Class
- Rich Domain Model (vs Anemic): Pure data structures with no behavior are called “anemic” and tend to push business logic into services, leading to scattered rules. By giving your entities methods—e.g. ChangeName(...)—you keep the logic that truly belongs with the entity right where it belongs.
- Self-Validation & Guarding Invariants:

Inside NotificationType.ChangeName, you can:
```csharp
if (string.IsNullOrWhiteSpace(newName))
    throw new ArgumentException("Name required", nameof(newName));
Name = newName;
```
No service or application layer needs to remember “never name it blank.” The entity enforces its own rules.
- Better Expressiveness & Intent

When reading application code you’ll see:
```csharp
notificationType.ChangeName("Urgent");
```
instead of

```csharp
notificationType.Name = "Urgent";
```
which makes it clear that you’re invoking a meaningful business operation, not just mutating a DTO.

### In Summary
- sealed → keeps your model bounded, prevents rogue subclasses, and signals completeness.
- private set → enforces encapsulation, ensuring all changes run through your guard-rails.
- Domain methods → enrich your entities with behavior, keep rules close to the data they govern, and avoid an anemic model.

Together, these patterns help your domain layer remain the single source of truth for business rules, making your application more maintainable, correct, and intention-revealing.

## DDD
These techniques are all core DDD techniques
- Rich Domain Model (Entities with behavior, invariants, encapsulation)
- Repositories as abstractions for persistence
- Ubiquitous Language (your NotificationType is a first-class concept)

How this aligns with DDD

| DDD Concept | What we did |
| --- | --- |
| Entity | sealed class NotificationType with identity and behavior (ChangeName) |
| Aggregate | You could treat NotificationType as a root, controlling its invariants |
| Repository | INotificationTypeRepository + EF-backed impl |
| Domain Service | (Not shown yet—would go here when logic spans multiple aggregates) |
| Value Object | You might add small types for e.g. NotificationTitle if you need stronger guarantees |
| Ubiquitous Language |	Names and methods in your Domain layer directly reflect your business terms |

## Clean Architecture vs. DDD
- Clean Architecture is about layering and dependency rules (inner layers know nothing of outer ones).
- DDD is about modeling the domain correctly (entities, aggregates, domain events, value objects, etc.) and applying both strategic (bounded contexts, context mapping) and tactical patterns.

What we’ve built is essentially tactical DDD inside a Clean-Architecture shell:

```pgsql
Api  → Infrastructure  → Application  → Domain
                     ↑––––––––––––––––––  
            (Repository impls using EF Core)
```

If you want to go “full DDD,” you’d augment this with:
- Aggregates and aggregate boundaries
- Domain Events for cross-aggregate or integration notifications
- Factories to encapsulate complex creation logic
- Domain Services for operations that don’t naturally belong on one entity
- Value Objects for richer types (dates, identifiers, titles)
- Strategic Design: bounded contexts, context maps, anti-corruption layers

### In short
- Yes: we’ve applied DDD’s tactical building blocks—rich entities, repositories, encapsulated invariants.
- Clean Architecture gives us the layers & dependency rules, while DDD gives us the modeling discipline.

So you’re not just doing “CRUD”; you’re already speaking the DDD dialect. If you need to deepen, start introducing aggregates, domain events, value objects, and strategic context boundaries.

## Why this structure?
- Single Generic Base – reduces boilerplate.
- One Repo per Aggregate – you can later add specialized queries (e.g. GetByNameAsync).
- DI in Infrastructure – keeps composition in one place.
- Endpoint Grouping – each entity’s routes live together, improving discoverability and testability.

## Application layer
Keeping your mapping logic in the Application layer—via those ToDto() / ToDomain() extension methods—gives you several Clean-Architecture and maintainability wins:
- Single Responsibility & Separation of Concerns
  - Endpoints stay focused on HTTP: routing, status codes, authorization, etc.
  - Application mappings handle only the translation between your Domain entities and the external DTO shapes.
  - If you ever change a DTO (e.g. rename a field, add a view-only property), you update one extension method in Application, not every endpoint.
- Compile-time Safety & Discoverability
  - Your mappings are plain C#. The compiler will flag any missing or mistyped fields.
  - IDE “Go To Definition” will take you directly to the ToDto() logic—no hunting through reflection-based profiles or JSON/XML conventions.
- Reusability & Consistency
  - Whether you expose data via Minimal APIs today or add a gRPC service tomorrow, you reuse the same ToDto()/ToDomain() calls.
  - You guarantee that all endpoints present your entities in exactly the same shape.
Testability
  - You can unit-test each mapping extension in isolation, asserting that every property is carried across correctly.
  - Your endpoints become trivially testable too, since they simply call repo.ListAsync().Select(p=>p.ToDto()).

## Infrastructure layer
- Take care of your EF Core setup, migrations, and database access here.
- Scaffold your database into this layer using EF Core tools. This gives you a clean separation between your domain model and the database schema.
- Use the generic repository pattern to abstract away the EF Core details. This allows you to swap out the underlying data access technology in the future if needed.
- Keep your DbContext and EF Core configurations here. This keeps your domain layer clean and focused on business logic.
- Use dependency injection to inject your DbContext and repositories into your application layer. This allows you to easily swap out implementations for testing or other purposes.

**IMPORTANT** 

These resources helped out to understand how separate the EF Data models to Clean Architecture Entities using Repository Pattern:
- [Practical clean architecture with EF Core database first](https://www.youtube.com/watch?v=YJ-PGIJowVQ&t=64s&ab_channel=Codewrinkles).
- https://stackoverflow.com/questions/60335165/with-entity-framework-core-database-first-approach-how-do-i-separate-my-entity-f

## Presentation layer
### Why not use Carter (or another routing/mapping library)?
Carter is fantastic for organizing routes into modules and providing model-binding, but:
- It doesn’t replace your need for DTO ↔ Domain mapping: Carter helps you group endpoints, apply filters, and bind parameters cleanly—but under the covers you still need to translate between your internal entities and the external contract. Your ToDto() extensions remain the natural place for that.
- Zero extra dependencies vs. “magic.”: By writing your own extension methods, there’s no hidden conventions or runtime scanning. Every field is explicit. Adding Carter adds another abstraction layer and its own conventions you and your team must learn and maintain.
- Finer-grained control: If a single endpoint needs to enrich a DTO with some custom logic (e.g. mask a field, compute a summary), you can simply drop into the extension or the endpoint itself. With profile-based mappers you often have to configure a special case or a custom resolver.