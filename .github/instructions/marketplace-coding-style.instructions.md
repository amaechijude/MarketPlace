---
name: Marketplace coding style
description: Follow the Marketplace repository conventions when implementing or modifying API features and their tests.
applyTo: "MarketPlace.Api/**/*.cs,MarketPlace.Test/**/*.cs"
---

# Marketplace Feature Development

Follow the existing .NET 10 style and patterns in this repository. Before implementing a feature, inspect the nearest comparable feature and match its organization, naming, dependency injection, response handling, and test approach.

## Feature organization

- Put API behavior under `MarketPlace.Api/Features/<FeatureName>`.
- Keep each use case together with its request, response, validator, handler, and endpoint when that matches the neighboring feature.
- Add or update the feature's `IRequestEndpoints` implementation so it is discovered by `Program.cs` through `MapRequestEndpoints()`.
- Use static endpoint mapping classes with a `Map(RouteGroupBuilder group)` method for individual operations.
- Use route groups, tags, authorization requirements, rate-limit policies, content types, validation, and response metadata consistently with nearby endpoints.

## Handlers and dependency injection

- Handlers are registered by assembly scanning in `Common/Extensions/**HandlersExtension.cs`; do not add manual registrations when a marker applies.
- Implement `IScopedRequestHandler` for scoped handlers, `ITransientMarker` for transient handlers, and `ISingletonMarker` for singleton handlers.
- Treat marker selection as part of the handler's design. Singleton handlers must not depend on scoped services or request state.
- Use primary constructors and `sealed` types where established by the surrounding code.
- Inject `AppDbContext`, infrastructure clients, caches, loggers, and options through constructors rather than resolving them manually.

## Requests, responses, and persistence

- Use nullable reference types and the repository's implicit usings; do not add redundant imports or disable nullable analysis.
- Keep endpoint delegates thin. Put business rules and persistence operations in handlers or the owning domain entity.
- Prefer `ApiResponse<T>` from `Common/ApiResponseFactory` and convert it with `ToMinimalApiResult()`.
- Apply `WithValidation<TRequest>()` to validated request-body or form endpoints and preserve the existing FluentValidation registration.
- Propagate `CancellationToken` to every asynchronous database, cache, and external-service operation.
- Use `AsNoTracking()` for read-only EF Core queries and preserve existing entity configuration and migration conventions.
- Return appropriate typed results and document success and problem responses with `.Produces(...)` or the existing response helper.
- Use the existing exception handler and Problem Details flow instead of introducing ad hoc error envelopes.

## Tests and verification

- Add focused xUnit tests for new behavior, following the existing naming and assertion style.
- Use `WebApplicationFactory<Program>` for endpoint/integration behavior and the existing test setup for replacing infrastructure dependencies.
- Keep unit tests independent of external services; use the repository's in-memory or test-container approach where appropriate.
- Before finishing, run the narrowest relevant test first, then `dotnet test MarketPlace.slnx` and an appropriate build or format check.
- Keep changes scoped to the feature and its tests. Do not refactor unrelated code or introduce a new abstraction without a concrete need.