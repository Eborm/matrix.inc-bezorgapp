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
    
    public async Task<List<Order>> GetOrdersAsync()
    {
        try
        {
            var ordersResponse = await _httpClient.GetAsync("/api/Order");
            if (!ordersResponse.IsSuccessStatusCode) return new List<Order>();
            var orders = await ordersResponse.Content.ReadFromJsonAsync<List<Order>>();

            var deliveryStatesResponse =
                await _httpClient.GetAsync($"{_baseUrl}/Api/DeliveryStates/GetAllDeliveryStates");
            if (!deliveryStatesResponse.IsSuccessStatusCode) return orders;
            var allDeliveryStates = await deliveryStatesResponse.Content.ReadFromJsonAsync<List<DeliveryState>>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var statesByOrderId = allDeliveryStates.GroupBy(ds => ds.OrderId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var order in orders)
            {
                if (statesByOrderId.TryGetValue(order.Id, out var orderStates))
                {
                    var lastStateWithService = orderStates.LastOrDefault(s => s.DeliveryService != null);
                    order.DeliveryServiceName = lastStateWithService?.DeliveryService?.Name ?? "Onbekend";

                    if (lastStateWithService != null)
                    {
                        order.DeliveryServiceId = lastStateWithService.DeliveryServiceId;
                    }

                    switch (orderStates.Count)
                    {
                        case 3:
                            order.DeliveryStateState = 2; // Afgeleverd
                            break;
                        case 2:
                            order.DeliveryStateState = 1; // Onderweg
                            break;
                        default:
                            order.DeliveryStateState = 0; // In afwachting
                            break;
                    }
                }
                else
                {
                    order.DeliveryServiceName = "Onbekend";
                    order.DeliveryStateState = 0;
                }
            }

            return orders;
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