using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System.Globalization;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace bezorgapp;

public partial class MapPage : ContentPage
{
    private readonly string orsApiKey = "5b3ce3597851110001cf6248969d1894c7d94094bab990a29141afca";

    private readonly Dictionary<string, string> AdressesToCords = new Dictionary<string, string>
    {
        { "warhouse",    "50.85056910665486, 6.011646432851468" },
        { "123 Elm St",  "50.892543417163814, 5.98174669784661" },
        { "456 Oak St",  "50.882447992696584, 5.98098648461065" },
        { "789 Pine St", "50.87175367817815, 5.998289390834275" }
    };

    private readonly List<(double lon, double lat)> deliveryLocations;

    public MapPage()
    {
        InitializeComponent();

        deliveryLocations = new List<(double lon, double lat)>
        {
            AdressToCoordinates("123 Elm St")[0],
            AdressToCoordinates("456 Oak St")[0],
            AdressToCoordinates("789 Pine St")[0]
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await CalculateRoute(deliveryLocations);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CalculateRoute: {ex.Message}");
            await DisplayAlert("Error", $"Unexpected error: {ex.Message}", "OK");
        }
    }

    private async Task<List<(double lon, double lat)>> OptimizeRouteAsync(List<(double lon, double lat)> locations)
    {
        var jobLocationMap = new Dictionary<int, (double lon, double lat)>();
        var jobs = locations.Select((loc, index) =>
        {
            int jobId = index + 1;
            jobLocationMap[jobId] = loc;
            return new
            {
                id = jobId,
                location = new[] { loc.lon, loc.lat }
            };
        }).ToArray();

        var vehicle = new
        {
            id = 1,
            profile = "driving-car",
            start = new[] { locations[0].lon, locations[0].lat },
            end = new[] { locations.Last().lon, locations.Last().lat }
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
            using var doc = JsonDocument.Parse(resultJson);
            {
                var root = doc.RootElement;

                var OptimezedRoute = new List<(double lon, double lat)>();

                if (root.TryGetProperty("routes", out JsonElement routesElement) && routesElement.ValueKind == JsonValueKind.Array)
                {
                    if (routesElement.EnumerateArray().Any())
                    {
                        var firstRoute = routesElement.EnumerateArray().First();
                        if (firstRoute.TryGetProperty("steps", out JsonElement stepsElement) && stepsElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var step in stepsElement.EnumerateArray())
                            {
                                if (step.TryGetProperty("type", out JsonElement typeElement) &&
                                    typeElement.GetString() == "job")
                                {
                                    if (step.TryGetProperty("id", out JsonElement idElement) &&
                                        idElement.ValueKind == JsonValueKind.Number)
                                    {
                                        int jobId = idElement.GetInt32();
                                        if (jobLocationMap.TryGetValue(jobId, out var originalLocation))
                                        {
                                            OptimezedRoute.Add(originalLocation);
                                        }
                                        else
                                        {
                                            Debug.WriteLine($"Warning: ORS returned job ID {jobId} not found in our map.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (root.TryGetProperty("unassigned", out JsonElement unassignedElement) && unassignedElement.EnumerateArray().Any())
                {
                    var unassignedJobIds = string.Join(", ", unassignedElement.EnumerateArray().Select(u => u.GetProperty("id").GetInt32()));
                    await DisplayAlert("Optimization Warning", $"Some jobs could not be assigned: {unassignedJobIds}", "OK");
                }

                return OptimezedRoute; 
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Error", $"ORS failed: {response.StatusCode}\n{error}", "OK");
            return locations;
        }
    }

    private List<(double lon, double lat)> AdressToCoordinates(string address)
    {
        if (AdressesToCords.TryGetValue(address, out var coords))
        {
            var parts = coords.Split(',');
            double lat = double.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            double lon = double.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);

            return new List<(double lon, double lat)> { (lon, lat) };
        }
        return new List<(double lon, double lat)>();
    }

    private async Task<List<(double lon, double lat)>> GetLocationAsync()
    {
        try
        {
            Location location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location != null)
                return new List<(double lon, double lat)> { (location.Longitude, location.Latitude) };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting location: {ex.Message}");
        }
        return new List<(double lon, double lat)>();
    }
    
    private async Task CalculateRoute(List<(double lon, double lat)> locations)
    {
        var start_end = AdressToCoordinates("warhouse");
        var fullRouteUnoptimized = new List<(double lon, double lat)>(locations);
        var currentLocation = await GetLocationAsync();

        if (currentLocation.Any())
        {
            fullRouteUnoptimized.Insert(0, currentLocation[0]);
        }
        
        fullRouteUnoptimized.Add(start_end[0]);
        
        var optimizedRoutePoints = await OptimizeRouteAsync(fullRouteUnoptimized);

        var payload = new
        {
            coordinates = optimizedRoutePoints.Select(loc => new[] { loc.lon, loc.lat }).ToArray(),
        };
        var json = JsonSerializer.Serialize(payload);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", orsApiKey);

        var response = await httpClient.PostAsync(
            "https://api.openrouteservice.org/v2/directions/driving-car/geojson",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        if (response.IsSuccessStatusCode)
        {
            var resultJson = await response.Content.ReadAsStringAsync();
            DrawRouteOnMap(resultJson, optimizedRoutePoints);
            await DisplayAlert("Route Calculated", "Successfully fetched and displayed route!", "OK");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"ORS Directions API error: {error}");
            await DisplayAlert("Error", $"ORS failed: {response.StatusCode}\n{error}", "OK");
        }
    }

    private void DrawRouteOnMap(string geoJson, List<(double lon, double lat)> stops)
    {
        mapView.MapElements.Clear();
        
        var polyline = new Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 8
        };

        using (JsonDocument doc = JsonDocument.Parse(geoJson))
        {
            var features = doc.RootElement.GetProperty("features");
            var geometry = features[0].GetProperty("geometry");
            var coordinates = geometry.GetProperty("coordinates");

            foreach (var coordinate in coordinates.EnumerateArray())
            {
                var longitude = coordinate[0].GetDouble();
                var latitude = coordinate[1].GetDouble();
                polyline.Geopath.Add(new Location(latitude, longitude));
            }
        }
        
        mapView.MapElements.Add(polyline);

        // Add pins for the stops
        for (int i = 0; i < stops.Count; i++)
        {
            var stop = stops[i];
            var pin = new Pin
            {
                Label = $"Stop {i + 1}",
                Location = new Location(stop.lat, stop.lon),
                Type = PinType.Place
            };
            mapView.Pins.Add(pin);
        }
        
        if (polyline.Geopath.Any())
        {
            mapView.MoveToRegion(MapSpan.FromCenterAndRadius(polyline.Geopath[0], Distance.FromKilometers(2.0)));
        }
    }
}