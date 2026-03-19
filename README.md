# AspNetReact Demo

A full-stack activity management application built with ASP.NET Core 7 and React 18 (TypeScript), used as a testing reference project.

## Application Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 7, MediatR, FluentValidation, AutoMapper |
| Auth | ASP.NET Core Identity, JWT (HS512) |
| Database | Entity Framework Core, SQLite |
| Frontend | React 18, TypeScript, MobX, Vite, Semantic UI |

---

## Testing Projects

Three projects make up the test suite. `Tests.Client` is a shared library; `Tests.API` and `Tests.E2E` both depend on it.

```
Tests.Client   ← NSwag-generated typed client + ApiClientFactory
     ├── Tests.API    ← in-process integration tests
     └── Tests.E2E    ← Playwright end-to-end tests
```

---

### Tests.Client — Shared API Client

A class library that owns the NSwag-generated typed C# client, a factory for creating instances of it, fluent DTO builders, and API-backed Object Mothers. Both test projects depend on this library so API calls, test data construction, and shared test objects are consistent everywhere.

**`ApiClient.g.cs`** is auto-generated from `swagger.json` before every build via an MSBuild NSwag target. The committed copy is used in CI if NSwag is not installed globally.

**`ApiClientFactory`** provides three factory methods:

| Method | Used by |
|---|---|
| `CreateAnonymous(baseUrl)` | E2E tests — unauthenticated calls to a live server |
| `CreateAuthenticated(baseUrl, token)` | E2E tests — authenticated calls to a live server |
| `Wrap(baseUrl, httpClient)` | API tests — wraps `WebApplicationFactory.CreateClient()` for in-process calls |

To refresh `swagger.json` from a running API:
```bash
dotnet swagger tofile -output Tests.Client/swagger.json http://localhost:5000/swagger/v1/swagger.json
```

#### Builders

Fluent builders for each request DTO. Every builder starts from sensible defaults so tests only declare what is relevant to them. Mutations are queued and applied to a fresh default on each `Build()` call, so the same builder instance is safe to reuse.

```csharp
// Use defaults
var dto = new ActivityFormDtoBuilder().Build();

// Override specific fields
var dto = new ActivityFormDtoBuilder()
    .Set(x => x.Title, "Custom Title")
    .Set(x => x.City, "Paris")
    .Build();
```

| Builder | DTO |
|---|---|
| `ActivityFormDtoBuilder` | `ActivityFormDto` |
| `LoginDtoBuilder` | `LoginDto` |
| `RegisterDtoBuilder` | `RegisterDto` |

#### Object Mothers

Instance classes that call the real API to create objects and cache the results. Repeated calls for the same key return the same `ActivityDto` / `UserDto` without hitting the API again. Mothers require an authenticated `IClient` and are initialised automatically by `PageTestBase` after login.

```csharp
// Cached — API called once, same object returned on every subsequent call
var activity = await Activities.DefaultAsync();
var activity = await Activities.GetOrCreateAsync("paris", () =>
    new ActivityFormDtoBuilder().Set(x => x.City, "Paris").Build());

// Not cached — new API call every time, for data that must be unique per test
var activity = await Activities.CreateOnceAsync(
    new ActivityFormDtoBuilder().Set(x => x.Title, $"Delete Me {Guid.NewGuid()}").Build());
```

| Mother | Creates via API | Cache key |
|---|---|---|
| `ActivityMother` | `CreateActivityAsync` | string key passed to `GetOrCreateAsync` |
| `UserMother` | `RegisterAsync` | string key passed to `GetOrCreateAsync` |

---

### Tests.API — API Integration Tests

In-process integration tests that spin up the real application using `WebApplicationFactory<Program>` with an in-memory database. Tests call the API through the `IClient` interface from `Tests.Client`, so they exercise the same contract as real consumers.

#### How it works

- **`ApiFactory`** — overrides the EF Core database with a named in-memory instance and sets a deterministic JWT key. A single factory instance is shared across all tests in the collection via `ICollectionFixture<ApiFactory>`.
- **`ApiTestBase`** — implements `IAsyncLifetime`. Before each test it registers the test user (idempotent), logs in, and provides two ready-to-use clients:
  - `Anon` — unauthenticated, backed by `ApiClientFactory.Wrap`
  - `Auth` — pre-authenticated with a Bearer token, backed by `ApiClientFactory.Wrap`

#### Test classes

| Class | What it covers |
|---|---|
| `AccountTests` | Register, Login, GetCurrentUser — both success and auth failure paths |
| `ActivitiesTests` | Full CRUD — list, get, create, edit, delete — with auth and anon variants |
| `ValidationTests` | FluentValidation rules enforced by the MediatR pipeline (400 responses) |

#### Running

```bash
dotnet test Tests.API
```

No running server required. Everything runs in-process.

---

### Tests.E2E — End-to-End Browser Tests

Playwright tests that drive a real browser against live instances of both the API and the React frontend. All API calls (login, registration, test data seeding) go through `Tests.Client` rather than raw HTTP, keeping them consistent with the API test suite.

Tests are organized using the Page Object Model and grouped into two parallel collections, each backed by an isolated credential pool to prevent account conflicts during concurrent execution.

#### Architecture

**Credential pools**

Each parallel collection is assigned its own pool of accounts from `testsettings.json`. Tests in `Playwright.1` and `Playwright.2` can run simultaneously without any risk of one test's login invalidating another's session.

```
Pools
├── Pool1 → SysAdmin: sysadmin1@test.com  |  StandardUser: user1@test.com
└── Pool2 → SysAdmin: sysadmin2@test.com  |  StandardUser: user2@test.com
```

**Collections**

| Collection | Pool | Test classes |
|---|---|---|
| `Playwright.1` | Pool1 | `ActivityCrudTests`, `ActivityListTests` |
| `Playwright.2` | Pool2 | `AuthTests`, `NavigationTests` |

**`PlaywrightFixture`** (one per collection) owns the browser process and exposes `GetAccount(AutomationAccount)`, which resolves credentials from the collection's assigned pool.

**`PageTestBase`** provides each test with a fresh `IBrowserContext` and `IPage`, then tears them down after the test. It reads `[UseAccount]` from the running test method (or the class as a fallback) via xUnit's `ITestOutputHelper` and automatically logs in before the test body runs.

#### `[UseAccount]` attribute

Declares which account a test runs as. Resolved at the method level first, then the class level. A single test class can freely mix accounts across its methods.

```csharp
[Fact]
[UseAccount(AutomationAccount.SysAdmin)]
public async Task CreateActivity_WithValidData_AppearsOnDetailPage() { ... }

[Fact]
[UseAccount(AutomationAccount.StandardUser)]
public async Task StandardUser_CanViewActivities() { ... }
```

Because account lookup goes through `Fixture.GetAccount(...)`, the same attribute on a test in `Playwright.1` resolves to `sysadmin1@test.com` and on a test in `Playwright.2` resolves to `sysadmin2@test.com`.

#### Authentication strategy

Tests that need an authenticated session bypass the login UI. `PageTestBase` calls `ApiClientFactory.CreateAnonymous` to register the account (idempotent), then `LoginAsync` to obtain a JWT, which is injected directly into `localStorage`. This keeps auth concerns out of non-auth tests and avoids driving the form on every test setup.

Tests in `AuthTests` that specifically cover the login/logout UI flow drive the form directly and do not use `[UseAccount]`.

#### Test data

`PageTestBase` exposes two ways to work with test data after login:

**Mothers** (`Activities`, `Users`) — API-backed Object Mothers available as properties on `PageTestBase`. Each mother holds its own `IClient` (the browser session's token) and a cache. Use these for shared data that should be created once and reused:

```csharp
var activity = await Activities.DefaultAsync();
var activity = await Activities.GetOrCreateAsync("key", () => new ActivityFormDtoBuilder().Build());
```

**`SeedActivityAsync`** — creates a one-off activity directly via the API for tests that need isolated, unique data. Pass a builder-constructed DTO or omit it to fall back to `ActivityMother.Default`:

```csharp
await SeedActivityAsync(new ActivityFormDtoBuilder().Set(x => x.Title, $"Delete Me {Guid.NewGuid()}").Build());
```

**`GetApiAsync()`** — performs its own independent login for the current test account and returns an authenticated `IClient` for ad-hoc API calls not covered by a mother. Because JWT is stateless, the separate token this creates coexists with the browser session token without conflict:

```csharp
var apiClient = await GetApiAsync();
var activity  = await apiClient.GetActivityAsync(id);
```

#### Page objects

Page objects wrap Playwright locators and expose typed actions. Methods that cause navigation return the destination page object, enabling fluent chains:

```csharp
var detailsPage  = await activitiesPage.ClickViewAsync(title);
var editFormPage = await detailsPage.ClickEditAsync();
var updatedPage  = await editFormPage.SubmitAsync();
```

All locators use `data-testid` attributes rather than CSS classes or text, so test selectors are stable across UI refactors.

#### Configuration

`testsettings.json` sets the target URLs, browser options, and account pools. `ApiUrl` is the server root — the NSwag client appends the correct `/api/...` paths from the swagger spec. Override locally with `testsettings.local.json` (gitignored). Environment variables prefixed `E2E_` also override settings, which is how CI injects values.

```json
{
  "BaseUrl": "http://localhost:3000",
  "ApiUrl": "http://localhost:5000",
  "Headless": true,
  "SlowMo": 0,
  "Pools": {
    "Pool1": {
      "SysAdmin":     { "Email": "sysadmin1@test.com", "Password": "..." },
      "StandardUser": { "Email": "user1@test.com",     "Password": "..." }
    },
    "Pool2": {
      "SysAdmin":     { "Email": "sysadmin2@test.com", "Password": "..." },
      "StandardUser": { "Email": "user2@test.com",     "Password": "..." }
    }
  }
}
```

#### Running

Both the API and the frontend must be running before executing the E2E suite.

```bash
# Terminal 1 — API
dotnet run --project API

# Terminal 2 — Frontend
cd client-app && npm run dev

# Terminal 3 — Tests
dotnet test Tests.E2E
```

To run headed or with slow motion for debugging, create `Tests.E2E/testsettings.local.json`:

```json
{
  "Headless": false,
  "SlowMo": 500
}
```

---

## CI Pipeline

Three jobs run on every push and pull request to `main`.

| Job | What it does |
|---|---|
| `build-api` | Builds the solution and runs all `Tests.API` integration tests |
| `build-client` | Type-checks and builds the React app |
| `test-e2e` | Starts both servers, waits for health checks, then runs `Tests.E2E` (depends on jobs 1 & 2) |

Test results and Playwright failure traces are uploaded as artifacts.
