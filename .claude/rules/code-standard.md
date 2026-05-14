A# Code Standard

These rules define the coding standards for this project. All code must follow these conventions to ensure consistency, maintainability, and clarity across the codebase.

## Global Usings

Global usings must be ordered in two groups separated by a blank line:

1. Third-party namespaces (alphabetical)
2. Project namespaces starting with `MadWorldEU.Byakko` (alphabetical)

```csharp
global using Microsoft.Extensions.DependencyInjection;
global using NodaTime;

global using MadWorldEU.Byakko.Common;
global using MadWorldEU.Byakko.Functional;
global using MadWorldEU.Byakko.Storages;
```

- Never mix third-party and project namespaces in the same group.
- Standard .NET namespaces (e.g. `System.*`) do not need to be in `GlobalUsings.cs` as implicit usings cover them.

## Namespaces

- The root namespace is always `MadWorldEU.Byakko`.
- After the root, the namespace must reflect the folder structure of the file (e.g. a file in `Storages/` uses `MadWorldEU.Byakko.Storages`).
- Never use the project name as part of the namespace — only the folder path after the project root.

## Access Modifiers

Use the most restrictive access modifier possible. Prefer in this order:

1. `private` — default for members not used outside their type
2. `internal` — for types and members used only within the same project
3. `public` — only when the type or member forms part of a public API or is required by a framework

Apply this to classes, methods, properties, and fields alike. Never widen access just for convenience — if something does not need to be public, it must not be.

## Classes

- Classes must be `sealed` by default unless they are explicitly designed for inheritance.
- Only omit `sealed` when a class is a base class intended to be extended.

## EF Core Constructor

Every entity must have a private parameterless constructor for EF Core. It must follow this exact pattern:

```csharp
/// <summary>
/// Required for EF Core
/// </summary>
[UsedImplicitly]
private MyClass() {}
```

- The `[UsedImplicitly]` attribute suppresses IDE warnings about unused constructors.
- The summary explains why the constructor exists.
- Never remove or make this constructor internal/public — EF Core requires it via reflection.

## Error Definitions

- All `Error` instances must be defined as `static readonly` fields on a dedicated static class named `{Domain}Errors` (e.g. `AssetErrors`).
- Error codes must follow the format `"Domain.Reason"` (e.g. `"Asset.NotFound"`), matching the class and field name.
- Never use inline `Error.Create(...)` calls at the call site — always reference a named field from the errors class.

## Date and Time

- Always use **NodaTime** types for date and time values. Never use `DateTime`, `DateTimeOffset`, or `TimeSpan`.
- Use `Instant` for UTC timestamps (e.g. `CreatedAt`, `UpdatedAt`).
- Use `ZonedDateTime` only when the timezone is meaningful to the domain.
- Inject `IClock` into use cases and services instead of calling `SystemClock.Instance` directly — this keeps time testable.

## Documentation

- All `public` and `internal` classes and methods must have a meaningful XML `<summary>` doc comment.
- Summaries must describe intent or behavior, not just restate the name.

## Configuration Binding

- Always use `GetSection(key).Get<T>() ?? throw new InvalidOperationException(...)` for required configuration sections. Never use `GetValue<T>` for complex objects — it only works for scalar values.
- The property names on the options class must exactly match the keys in `appsettings.json` for binding to work.
- Required sections that are absent should throw at startup, not silently default.
