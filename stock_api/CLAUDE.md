# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

An ASP.NET Core 6.0 minimal-hosting Web API (`stock_api`) that mixes top-level minimal API endpoints (in `Program.cs`) with a traditional MVC controller (`Controllers/`). It backs a stock order system: JWT-based login and order create/read endpoints, plus the default `WeatherForecast` scaffold left over from `dotnet new webapi`.

## Commands

```bash
dotnet build                 # build
dotnet run                   # run the API (Swagger UI at /swagger in the default launch profile)
dotnet watch run             # run with hot reload
```

There is no test project in this repo yet.

EF Core CLI tools (`dotnet-ef`) are referenced via `Microsoft.EntityFrameworkCore.Tools` but no `Migrations/` folder exists — the database schema is not currently managed through EF migrations.

## Architecture

- **Two `DbContext` classes exist for the same tables, and only one is actually wired up.** `AppDbContext` is defined inline at the bottom of `Program.cs` and is the one registered in DI (`builder.Services.AddDbContext<AppDbContext>(...)`) and used by every endpoint — it has no `OnModelCreating`, so no column constraints are configured. `Data/StockDBContext.cs` defines the "real" constraints (max lengths, decimal precision, `CreatedAt` default) but is never registered or referenced anywhere. When changing entity configuration, know which context is actually live (`AppDbContext`) before assuming `StockDBContext`'s constraints apply.
- **Endpoints are minimal-API top-level statements in `Program.cs`**, not controllers, except for the scaffolded `WeatherForecastController`. New stock/order/user endpoints should follow the existing minimal-API style unless there's a reason to introduce a new controller.
- **Auth**: JWT bearer auth with a single hardcoded symmetric signing key (duplicated in two places in `Program.cs`). `/login` issues tokens against `Users` in the DB by plaintext username/password match. Tokens currently carry no claims (no subject/user id), so `.RequireAuthorization()` on the order endpoints only proves *a* valid token was presented, not *whose* — the `{userId}` route parameter is trusted as-is rather than derived from the token.
- **Models** (`Models/OrderModel.cs`, `Models/UserModel.cs`) are plain EF entity classes with no `[Required]`/`[Range]` data-annotation validation; whatever `AppDbContext`'s conventions produce is what's enforced.
- Connection string (`ConnectionStrings:DefaultConnection` in `appsettings.json`) points at a local SQL Server instance and is checked into source control as plaintext, including a commented-out alternate connection string — check with the user before changing or relying on it as-is.
