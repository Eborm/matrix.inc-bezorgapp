using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace bezorgapp.Models;

public class Customer
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}