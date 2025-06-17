using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using bezorgapp.Models;

namespace bezorgapp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "ENTERAPIKEY";
    private readonly string _baseUrl = "http://51.137.100.120:5000";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("apiKey", _apiKey);
    }

    public async Task<int> GetDeliveryStateByIdAsync(int Id)
    {
        DeliveryState OrderDeliveryState;

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Api/DeliveryStates/GetAllDeliveryStates");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var deliveryStates = JsonSerializer.Deserialize<List<DeliveryState>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var statesForOrder = deliveryStates
                .Where(ds => ds.OrderId == Id)
                .ToList();

            string serialized = JsonSerializer.Serialize(statesForOrder, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            if (statesForOrder.Count == 2)
            {
                return 1;
            }
            else if (statesForOrder.Count == 3)
            {
                return 2;
            }


        }
        catch (Exception ex)
        {
            return 0;   
        }
        return 0;
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/Order");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Order>>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching orders: {ex.Message}");
        }
        return new List<Order>();
    }
}
