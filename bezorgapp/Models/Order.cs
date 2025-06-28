using System.Text.Json.Serialization;

namespace bezorgapp.Models;

public class Order
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("customer")]
    public Customer Customer { get; set; }
    
    public string DeliveryState { get; set; }
}
