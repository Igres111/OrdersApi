# Orders API

A small ASP.NET Core Web API that processes an `orders.json` file. It can:

1. Display all orders
2. Find the orders of a specific customer
3. Calculate statistics for completed orders (cancelled orders are excluded):
   number of completed orders, total sales, average order value, and the most popular product by quantity sold

## How to run

**Requirement:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/Igres111/OrdersApi.git
cd OrdersApi
dotnet run --project OrdersApi
```

The API starts at `http://localhost:5264`.

| Endpoint | Description |
|---|---|
| `GET /api/orders` | All orders |
| `GET /api/orders/customer/{name}` | Orders of one customer, e.g. `/api/orders/customer/Nino` |
| `GET /api/orders/statistics` | Statistics for completed orders |

Ways to call the endpoints:

- Open the URLs in a browser, e.g. `http://localhost:5264/api/orders/statistics`
- Open `OrdersApi/OrdersApi.http` in Visual Studio or Rider and click **Send request**
- Use curl: `curl http://localhost:5264/api/orders/statistics`

Result of the statistics endpoint for the provided file:

```json
{
  "completedOrdersCount": 3,
  "totalSales": 425,
  "averageOrderValue": 141.67,
  "mostPopularProducts": ["Keyboard", "Mouse"],
  "mostPopularQuantity": 3
}
```

The input file is `OrdersApi/Data/orders.json`. To use a different file, change `OrdersFilePath` in `OrdersApi/appsettings.json`.

### Running the tests

```bash
dotnet test
```

There are 15 unit tests for the order logic, in `OrdersApi.Tests/OrderServiceTests.cs`.

## Project structure

```
OrdersApi/
├── Controllers/   OrdersController - the three endpoints
├── Services/      OrderService - customer search and all calculations
├── Data/          orders.json and JsonOrderSource, which reads it
├── Models/        Order, OrderItem, OrderStatistics (data only, no logic)
└── Enums/         OrderStatus
OrdersApi.Tests/   xUnit tests for OrderService
```

## AI tools used

- **Claude Code**, used as a coding assistant in the desktop app.
- **Chat GPT**, used as a guidance assistance.

## How AI helped

- Wrote an initial plan for the project and calculated the expected results by hand from the JSON file, before any code existed.
- Generated the first version of the models, the JSON loader, the service, the controller and the unit tests.
- Explained things I asked about, for example why wrapper Lazy<T> was used and how it suits to task needs.
- Ran the build, the tests and the endpoints after each change and reported the results.

I reviewed each step, asked questions, and changed the design and the code where I disagreed (see below).

## Errors and incorrect logic found in AI-generated code

- The biggest problem AI faced was the structure of the project. It did over-simplified it and was creating only 2-3 folders without proper structure.
- Instead of using classes for models, it used records which are not suitable for this case scenatio. No Dtos were needed to use records at all.
- Always uses ternary operators, which sometimes works poorly for code readability and future debugs.

## How I corrected the result
- Used personal experience to provide the best results. Asked AI to use dedicated folder structer, rewrite records to classes and to use simplified logic with if statements.
