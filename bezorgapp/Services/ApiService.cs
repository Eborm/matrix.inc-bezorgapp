using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using bezorgapp.Models;
using System;

namespace bezorgapp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "API_KEY_GOES_HERE";
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
                var filteredOrders = orders.Where(o => o.DeliveryServiceName == "Tempnaam").ToList();
                var extraorders = orders.Where(o => o.DeliveryServiceName == "Onbekend").ToList();
                foreach (var extraorder in extraorders)
                {
                    filteredOrders.Add(extraorder);
                    Console.WriteLine($"Added extra order {extraorder.Id}");
                }
                return filteredOrders;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching orders with details: {ex.Message}");
        }
        return new List<Order>();
    }

    public async Task<(bool Success, string ErrorMessage)> MarkAsInProgressAsync(int orderId)
    {
        var url = $"/api/DeliveryStates/StartDelivery?OrderId={orderId}";
        try
        {
            var response = await _httpClient.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = $"Statuscode: {response.StatusCode}\nServer-respons: {errorContent}";
                return (false, errorMessage);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Uitzondering: {ex.Message}");
        }
    }
    
    public async Task<(bool Success, string ErrorMessage)> MarkAsCompletedAsync(int orderId)
    {
        var url = $"/api/DeliveryStates/CompleteDelivery?OrderId={orderId}";
        try
        {
            var response = await _httpClient.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = $"Statuscode: {response.StatusCode}\nServer-respons: {errorContent}";
                return (false, errorMessage);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Uitzondering: {ex.Message}");
        }
    }
}