using System.Text.Json;
using System.Text.Json.Serialization;
using OrdersApi.Data.Interfaces;
using OrdersApi.Models;

namespace OrdersApi.Data
{
    public class JsonOrderSource : IOrderSource
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
        };

        private readonly string _filePath;
        private readonly Lazy<IReadOnlyList<Order>> _orders;

        public JsonOrderSource(string filePath)
        {
            _filePath = Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(AppContext.BaseDirectory, filePath);

            _orders = new Lazy<IReadOnlyList<Order>>(Load);
        }

        public IReadOnlyList<Order> GetOrders() => _orders.Value;

        private IReadOnlyList<Order> Load()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"Orders file not found: {_filePath}", _filePath);
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                var orders = JsonSerializer.Deserialize<List<Order>>(json, JsonOptions);

                return orders ?? throw new InvalidDataException($"Orders file is empty or null: {_filePath}");
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Orders file is not valid JSON or has an unexpected shape: {_filePath}. {ex.Message}", ex);
            }
        }
    }
}
