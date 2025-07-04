using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using bezorgapp.Models;
using System;

namespace bezorgapp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    public string BaseUrl { get; } = "https://bezorgapp-api-1234.azurewebsites.net";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }
    
    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Order/{orderId}");
            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<Order>();
                return order;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching order by ID: {ex.Message}");
        }
        return null;
    }
    
    public async Task<List<Order>> GetOrdersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/Order/GetMyDeliveries");
            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
                return orders ?? new List<Order>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching filtered orders: {ex.Message}");
        }
        return new List<Order>();
    }

    // Markeer een bestelling als 'In Progress' (onderweg)
    public async Task<(bool Success, string ErrorMessage)> MarkAsInProgressAsync(int orderId)
    {
        var url = $"/api/DeliveryStates/StartDelivery?OrderId={orderId}";
        try
        {
            var response = await _httpClient.PostAsync(url, null);
            if (response.IsSuccessStatusCode) return (true, null);
            
            string errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"Statuscode: {response.StatusCode}\nServer-respons: {errorContent}");
        }
        catch(Exception ex)
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
            if (response.IsSuccessStatusCode) return (true, null);
            
            string errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"Statuscode: {response.StatusCode}\nServer-respons: {errorContent}");
        }
        catch (Exception ex)
        {
            return (false, $"Uitzondering: {ex.Message}");
        }
    }
}