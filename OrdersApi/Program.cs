using System.Text.Json.Serialization;
using OrdersApi.Data;
using OrdersApi.Data.Interfaces;
using OrdersApi.Services;
using OrdersApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var ordersFilePath = builder.Configuration["OrdersFilePath"]
    ?? throw new InvalidOperationException("'OrdersFilePath' is not configured in appsettings.json.");

builder.Services.AddSingleton<IOrderSource>(new JsonOrderSource(ordersFilePath));
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.Services.GetRequiredService<IOrderSource>().GetOrders();

app.Logger.LogInformation("Loaded orders from {OrdersFilePath}", ordersFilePath);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
