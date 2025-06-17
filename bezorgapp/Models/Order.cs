using System.Text.Json.Serialization;

namespace bezorgapp.Models;

public class Order
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }

    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("customer")]
    public Customer Customer { get; set; }

    [JsonPropertyName("products")]
    public List<object> Products { get; set; }

    [JsonPropertyName("deliveryStates")]
    public List<object> DeliveryStates { get; set; }

    public int DeliveryStateState { get; set; } // 0 = In afwachting, 1 = Onderweg, 2 = Afgeleverd

    public string DeliveryState { get; set; }
}