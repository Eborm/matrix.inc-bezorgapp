using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using bezorgapp.Models;

namespace bezorgapp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "";
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
