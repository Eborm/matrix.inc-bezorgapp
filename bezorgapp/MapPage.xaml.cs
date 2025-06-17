using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace bezorgapp;

public partial class MapPage : ContentPage
{
    private readonly string orsApiKey = "5b3ce3597851110001cf6248969d1894c7d94094bab990a29141afca";

    private readonly Dictionary<string, string> deliveryLocations = new Dictionary<string, string>
    {
        { "warhouse","40.7518325848006, -74.00519833268147" },
        { "123 Elm St", "40.93387193082394, -73.89109905918852" },
        { "456 Oak St", "40.92778544896371, -73.89090211231711" },
        { "789 Pine St", "40.66591172473292, -73.86833455372528" } 
    };
    public MapPage()
    {
        InitializeComponent();
        var locations = new List<(double lon, double lat)>
        {
            (40.7518325848006, -74.00519833268147),
            (40.93387193082394, -73.89109905918852), 
            (40.92778544896371, -73.89090211231711), 
            (40.66591172473292, -73.86833455372528), 
        };
        OptimizeRouteAsync(locations);
    }





    private async Task OptimizeRouteAsync(List<(double lon, double lat)> locations)
    {
        var jobs = locations.Select((loc, index) => new
        {
            id = index + 1,
            location = new[] { loc.lon, loc.lat }
        }).ToArray();

        var vehicle = new
        {
            id = 1,
            start = new[] { locations[0].lon, locations[0].lat },
            end = new[] { locations[0].lon, locations[0].lat }
        };

        var payload = new
        {
            jobs = jobs,
            vehicles = new[] { vehicle }
        };

        var json = JsonSerializer.Serialize(payload);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", orsApiKey);

        var response = await httpClient.PostAsync(
            "https://api.openrouteservice.org/optimization",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        if (response.IsSuccessStatusCode)
        {
            var resultJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(resultJson);

            var steps = doc.RootElement
                .GetProperty("routes")[0]
                .GetProperty("steps");

            StringBuilder route = new();
            route.AppendLine("Optimal Stop Order:");

            foreach (var step in steps.EnumerateArray())
            {
                int id = step.GetProperty("id").GetInt32();
                route.AppendLine($"Stop #{id}");
            }

            await DisplayAlert("Optimized Route", route.ToString(), "OK");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Error", $"ORS failed: {response.StatusCode}\n{error}", "OK");
        }
    }
}
