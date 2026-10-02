# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Take-home assignment: an ASP.NET Core Web API (.NET 10) that processes an `orders.json` file. The features and unit tests are done; the `README.md` required by the assignment has not been added yet.

### Requirements (from the assignment)

1. Display all orders.
2. Find the orders for a specific customer.
3. Leave cancelled orders out of the statistics.
4. For completed orders, calculate: the number of completed orders, total sales, average order value, and the most popular product by quantity sold.
5. Deliverables: source code, **at least 2 tests**, and a `README.md` covering how to run the program, which AI tools were used and how they helped, any errors found in AI-generated code (or an explicit statement that there were none), how the result was verified and corrected, and the assumptions made for unclear cases.

Keep the scope small: no database, EF Core, auth, MediatR, custom exception middleware, or extra endpoints beyond the three below.

## Architecture

Two projects in `OrdersApi.slnx`: `OrdersApi` (the Web API) and `OrdersApi.Tests` (xUnit v2, references `OrdersApi`).

A request flows through three layers:

```
Data/orders.json -> JsonOrderSource (Data/) -> OrderService (Services/) -> OrdersController (Controllers/)
```

- **Data**: `JsonOrderSource` implements `Data/Interfaces/IOrderSource`. It reads the path from the `OrdersFilePath` setting in `appsettings.json` (resolved against `AppContext.BaseDirectory`; `orders.json` is copied to the output folder), deserializes the file once and caches it. It is registered as a singleton. `Program.cs` calls `GetOrders()` right after `builder.Build()` so that a missing or invalid file stops the app at startup. Do not remove that call, even though its result is unused.
- **Services**: `OrderService` implements `Services/Interfaces/IOrderService` and is registered as scoped. It holds all logic: customer search and every calculation, including a single order's total. It depends only on `IOrderSource`, with no HTTP or file I/O, so tests can feed it in-memory orders.
- **Controllers**: `OrdersController` is a thin pass-through with `GET /api/orders`, `GET /api/orders/customer/{name}` and `GET /api/orders/statistics`. `[ApiController]` rejects a blank name with a 400 before the action runs, so the controller has no `try/catch`.
- **Models** (`Models/`) are plain classes with `{ get; set; }` properties, shaped like the JSON, with no computed properties or logic. Keep calculations out of them.
- JSON: the file is read with case-insensitive property and enum names (`"completed"` -> `OrderStatus.Completed`). API responses return enums as strings through `AddJsonOptions` in `Program.cs`. These are two separate serializer configurations.
- Tests: `OrderServiceTests` feeds `OrderService` through a private `FakeOrderSource`. `SampleOrders()` mirrors `orders.json`, so expected values match the hand-calculated results below.

## Expected results for the provided data

Use these as the test oracle. They were computed by hand.

- Completed orders: #1, #3, #4. Order #2 (Giorgi) is cancelled and excluded.
- Completed order count: **3**. Total sales: 125 + 250 + 50 = **425**. Average: **141.67**.
- Most popular product by quantity: **Keyboard and Mouse tie at 3 each**. If cancelled orders were wrongly included, Keyboard would win with 4. A plain `.First()` after sorting silently hides the tie, so every tied product is returned in alphabetical order.

## Commands

Run from the solution root (`OrdersApi.slnx`, the new XML solution format; it needs the .NET 10 SDK):

```bash
dotnet build
dotnet run --project OrdersApi
dotnet test
dotnet test --filter "FullyQualifiedName~OrderServiceTests.GetStatistics_WhenProductsTie_ReturnsAllOfThemAlphabetically"
```

- The dev server listens on `http://localhost:5264` (or `https://localhost:7251` with `--launch-profile https`).
- There is no Swagger UI. The template uses `Microsoft.AspNetCore.OpenApi`, which serves only the raw document at `/openapi/v1.json` in Development. Add Swashbuckle or Scalar if an interactive UI is wanted.
- `OrdersApi/OrdersApi.http` holds one sample request per endpoint for the VS/Rider HTTP client.
