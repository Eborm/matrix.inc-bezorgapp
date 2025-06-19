using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace bezorgapp;

public partial class MapPage : ContentPage
{
    private readonly string orsApiKey = "5b3ce3597851110001cf6248969d1894c7d94094bab990a29141afca";

    private readonly Dictionary<string, string> AdressesToCords = new Dictionary<string, string>
    {
        { "warhouse","50.85056910665486, 6.011646432851468" },
        { "123 Elm St", "50.892543417163814, 5.98174669784661" },
        { "456 Oak St", "50.882447992696584, 5.98098648461065" },
        { "789 Pine St", "50.87175367817815, 5.998289390834275" }
    };

    private readonly List<(double lon, double lat)> deliveryLocations = new()
    {
        (-74.00519833268147, 40.7518325848006),
        (-73.86833455372528, 40.66591172473292),
        (-73.89109905918852, 40.93387193082394),
        (-73.89090211231711, 40.92778544896371),
    };

    public MapPage()
    {
        InitializeComponent();

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await OptimizeRouteAsync(deliveryLocations);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OptimizeRouteAsync: {ex.Message}");
            await DisplayAlert("Error", $"Unexpected error: {ex.Message}", "OK");
        }
        try
        {
            var currentLocation = await GetLocationAsync();
            string locationString = string.Join(", ", currentLocation.Select(loc => $"{loc.lon}, {loc.lat}"));
            await DisplayAlert("Error", $"location: {locationString}", "OK");
        }
        catch
        (Exception ex)
        {
            Debug.WriteLine($"Exception in GetLocationAsync: {ex.Message}");
            await DisplayAlert("Error", $"Unexpected error: {ex.Message}", "OK");
        }
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
            profile = "driving-car",
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
            Debug.WriteLine("ORS API Response:\n" + resultJson);

            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("routes", out var routesElement) && routesElement.ValueKind == JsonValueKind.Array)
            {
                StringBuilder routeSummary = new();
                routeSummary.AppendLine("Optimal Stop Orders:");

                foreach (var route in routesElement.EnumerateArray())
                {
                    if (route.TryGetProperty("vehicle", out var vehicleElement))
                    {
                        int vehicleId = vehicleElement.GetInt32();
                        routeSummary.AppendLine($"Vehicle #{vehicleId}:");
                    }
                    else
                    {
                        routeSummary.AppendLine("Vehicle: Unknown");
                    }

                    if (route.TryGetProperty("steps", out var stepsElement) && stepsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var step in stepsElement.EnumerateArray())
                        {
                            if (step.TryGetProperty("job", out var jobElement))
                            {
                                int jobId = jobElement.GetInt32();
                                routeSummary.AppendLine($"  Stop #{jobId}");
                            }
                        }
                    }
                    else
                    {
                        routeSummary.AppendLine("  No steps found.");
                    }
                }

                await DisplayAlert("Optimized Route", routeSummary.ToString(), "OK");
            }
            else
            {
                await DisplayAlert("Error", "Response JSON has no 'routes' property.", "OK");
            }

        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"ORS error response: {error}");
            await DisplayAlert("Error", $"ORS failed: {response.StatusCode}\n{error}", "OK");
        }
    }

    private List<(double lon, double lat)> AdressToCoordinatesAsync(string address)
    {
        if (AdressesToCords.TryGetValue(address, out var coords))
        {
            var parts = coords.Split(',');
            return new List<(double lon, double lat)>
            {
                (double.Parse(parts[0]), double.Parse(parts[1]))
            };
        }
        else
        {
            return new List<(double lon, double lat)>(); // Return empty if address not found
        }
    }

    private async Task<List<(double lon, double lat)>> GetLocationAsync()
    {
        Location location = await Geolocation.Default.GetLastKnownLocationAsync();
        return new List<(double lon, double lat)> { (location.Latitude, location.Longitude) };
    }
}
