# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

Take-home assignment: a small program that processes an `orders.json` file. Right now the repo is a stripped ASP.NET Core Web API template: the WeatherForecast sample is gone, `Data/orders.json` holds the input data (copied to the output folder). The enum, the models, the data loader and `OrderService` (`Services/`, implementing `Services/Interfaces/IOrderService`, registered as scoped) exist, along with `OrdersController`, which serves `GET /api/orders`, `/api/orders/customer/{name}` and `/api/orders/statistics`. Enums are returned as strings through `AddJsonOptions`. `OrdersApi.Tests` is an xUnit v2 project (net10.0) that references `OrdersApi`; `OrderServiceTests` holds 15 unit tests that feed `OrderService` in-memory orders through a private `FakeOrderSource`, so they don't depend on the JSON file.

Loading works like this: `JsonOrderSource` (in `Data/`, implementing `Data/Interfaces/IOrderSource`) reads the path from the `OrdersFilePath` setting in `appsettings.json`, resolved against `AppContext.BaseDirectory`. It deserializes the file once, caches it, and is registered as a singleton. `Program.cs` loads it at startup, so a missing or invalid file stops the app immediately. Status parsing is case-insensitive, and an unknown status value fails the load. It is not a git repository yet.

### Requirements (from the assignment)

1. Display all orders.
2. Find the orders for a specific customer.
3. Leave cancelled orders out of the statistics.
4. For completed orders, calculate: the number of completed orders, total sales, average order value, and the most popular product by quantity sold.
5. Deliverables: source code, **at least 2 tests**, and a `README.md` covering how to run the program, which AI tools were used and how they helped, any errors found in AI-generated code (or an explicit statement that there were none), how the result was verified and corrected, and the assumptions made for unclear cases.

`orders.json` shape: an array of `{ orderId, customer, status, items: [{ product, quantity, price }] }`. `status` is lowercase in the file (`"completed"`, `"cancelled"`).

### Expected results for the provided data

Use these as the test oracle. They were computed by hand.

- Completed orders: #1, #3, #4. Order #2 (Giorgi) is cancelled and excluded.
- Completed order count: **3**. Total sales: 125 + 250 + 50 = **425**. Average: **141.67**.
- Most popular product by quantity: **Keyboard and Mouse tie at 3 each**. If cancelled orders were wrongly included, Keyboard would win with 4. A plain `.First()` after sorting silently hides the tie, so return every tied product in alphabetical order.

### Decisions for unclear cases (document them in the README)

- Match customer names case-insensitively and trim whitespace. An unknown customer returns `200` with an empty list.
- Parse status case-insensitively. Unknown statuses appear in "all orders" but not in the statistics.
- If there are no completed orders, return zeros and an empty product list (no divide-by-zero).
- Use `decimal` for money. Round the average to 2 decimals with `MidpointRounding.AwayFromZero` (x.xx5 always rounds up).
- "Popular" means by quantity, not by revenue.

## Target structure (planned, not yet created)

Keep the scope small: no database, EF Core, auth, MediatR, or repository layers.

- `OrdersApi` (this Web API project): a thin controller plus composition in `Program.cs`. Endpoints: `GET /api/orders`, `GET /api/orders/customer/{name}`, `GET /api/orders/statistics`. The path to the orders file comes from `appsettings.json`.
- Models (`Models/`) are plain classes with `{ get; set; }` properties, shaped like the JSON, with no computed properties or logic. Every calculation, including a single order's total, lives in `Services/`.
- An `OrdersApi.Core`-style class library, or a `Services/` folder if staying single-project: models, a JSON loader, and a pure `OrderService` that takes the orders and returns results with no HTTP or file I/O, so it can be unit tested directly.
- An xUnit test project: unit tests on `OrderService` using in-memory data that mirrors `orders.json`, plus an optional integration test through `WebApplicationFactory<Program>` (this needs `public partial class Program {}` in `Program.cs`).

## Commands

Run from the solution root (`OrdersApi.slnx`, the new XML solution format; it needs the .NET 10 SDK):

```bash
dotnet build
dotnet run --project OrdersApi
dotnet test
dotnet test --filter "FullyQualifiedName~OrderServiceTests.GetStatistics_ReturnsAllProductsWhenTied"
dotnet sln OrdersApi.slnx add <path-to-csproj>
```

- The dev server listens on `http://localhost:5264` (or `https://localhost:7251` with `--launch-profile https`).
- There is no Swagger UI. The template uses `Microsoft.AspNetCore.OpenApi`, which serves only the raw document at `/openapi/v1.json` in Development. Add Swashbuckle or Scalar if an interactive UI is wanted.
- `OrdersApi/OrdersApi.http` holds sample requests for the VS/Rider HTTP client.
