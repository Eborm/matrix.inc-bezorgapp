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
            Console.WriteLine($"Error fetching delivery state: {ex.Message}");
        }
        return 0;
    }

    public async Task<string> GetDeliveryServiceNameById(int Id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Api/DeliveryStates/GetAllDeliveryStates");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var deliveryStates = JsonSerializer.Deserialize<List<DeliveryState>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var lastStateForOrder = deliveryStates
                .Where(ds => ds.OrderId == Id && ds.DeliveryService != null)
                .LastOrDefault();

            if (lastStateForOrder != null)
            {
                return lastStateForOrder.DeliveryService.Name;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching delivery service name: {ex.Message}");
        }
        return "Onbekend";
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/Order");
            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
                foreach (var order in orders)
                {
                    order.DeliveryServiceName = await GetDeliveryServiceNameById(order.Id);
                }
                return orders;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching orders: {ex.Message}");
        }
        return new List<Order>();
    }
}