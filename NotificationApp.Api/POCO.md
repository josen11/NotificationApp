# POCO
POCO stands for Plain Old CLR Object. It’s a play on the term “POJO” (“Plain Old Java Object”) from the Java world.

In .NET, a POCO is simply:
- A class or struct
- That doesn’t inherit from any framework-specific base class
- And doesn’t implement any framework-specific interfaces

It’s just your own “plain” type, with only the properties and methods you declare.

## Why POCOs matter
- Framework Independence: Your domain entities (or DTOs, or settings classes) remain decoupled from EF Core, ASP NET Core, or any other framework. You can move them between projects, test them in isolation, or use them in different scenarios without dragging framework dependencies along.
- Testability: Since they’re just simple CLR types, you can instantiate and manipulate them in unit tests without any special setup.
- Clarity of Design: When you see a POCO, you know exactly where your business logic lives—inside your own code—rather than hidden inside framework base classes or generated proxies.

## Example
```csharp
// This is a POCO.
public class NotificationType
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    public NotificationType(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
```
Compare that to a non-POCO, which might derive from an EF Core base type or implement EF-specific interfaces—something you generally want to avoid in your core/domain models.

In short, POCO simply means “a plain .NET object with no framework baggage.”