using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Controls;
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
            int jobId = index + 1; // ORS job IDs are 1-based
            jobLocationMap[jobId] = loc; // Store the original location with its ID
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
                        var firstRoute = routesElement.EnumerateArray().First(); // Get the first (and usually only) route
                        if (firstRoute.TryGetProperty("steps", out JsonElement stepsElement) && stepsElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var step in stepsElement.EnumerateArray())
                            {
                                if (step.TryGetProperty("type", out JsonElement typeElement) &&
                                    typeElement.GetString() == "job") // We only care about "job" steps for the sequence
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
                    else
                    {
                        Debug.WriteLine("ORS response contains 'routes' array, but it's empty.");
                    }
                }
                else
                {
                    Debug.WriteLine("ORS response does not contain a 'routes' array or it's not an array.");
                }

                // Check for unassigned jobs (optional, but good for debugging)
                if (root.TryGetProperty("unassigned", out JsonElement unassignedElement) && unassignedElement.ValueKind == JsonValueKind.Array)
                {
                    if (unassignedElement.EnumerateArray().Any())
                    {
                        var unassignedJobIds = string.Join(", ", unassignedElement.EnumerateArray().Select(u => u.GetProperty("id").GetInt32()));
                        await DisplayAlert("Optimization Warning", $"Some jobs could not be assigned: {unassignedJobIds}", "OK");
                        Debug.WriteLine($"Unassigned ORS jobs: {unassignedJobIds}");
                    }
                }

                return OptimezedRoute; // <--- This returns the reordered list!
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"ORS error response: {error}");
            await DisplayAlert("Error", $"ORS failed: {response.StatusCode}\n{error}", "OK");
            return locations; // Return the original order if optimization fails
        }
    }

    private List<(double lon, double lat)> AdressToCoordinates(string address)
    {
        if (AdressesToCords.TryGetValue(address, out var coords))
        {
            var parts = coords.Split(',');
            // Use CultureInfo.InvariantCulture to correctly parse the doubles
            double lat = double.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            double lon = double.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);

            return new List<(double lon, double lat)> { (lon, lat) };
        }
        return new List<(double lon, double lat)>();
    }

    private async Task<List<(double lon, double lat)>> GetLocationAsync()
    {
        Location location = await Geolocation.Default.GetLastKnownLocationAsync();
        return new List<(double lon, double lat)> { (location.Longitude, location.Latitude) };
    }

    private async Task CalculateRoute(List<(double lon, double lat)> Locations)
    {
        var start_end = AdressToCoordinates("warhouse");
        var fullRoute = new List<(double lon, double lat)>(Locations);
        var curruntlocation= await GetLocationAsync();
        fullRoute.Insert(0, (curruntlocation[0]));
        fullRoute.Add(start_end[0]);
        var fullroute = await OptimizeRouteAsync(fullRoute);

        var payload = new
        {
            coordinates = fullroute.Select(loc => new[] { loc.lon, loc.lat }).ToArray(),
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

            var geoJsonDoc = JsonDocument.Parse(resultJson);

            // Extract coordinates
            var coordinates = geoJsonDoc.RootElement
                .GetProperty("features")[0]
                .GetProperty("geometry")
                .GetProperty("coordinates")
                .EnumerateArray()
                .Select(coord =>
                {
                    double lon = coord[0].GetDouble();
                    double lat = coord[1].GetDouble();
                    return new Location(lat, lon); // MAUI Maps expects (lat, lon)
                }).ToList();

            // Draw polyline
            var polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5
            };

            foreach (var location in coordinates)
            {
                polyline.Geopath.Add(location);
            }
            foreach (var loc in fullroute)
            {
                var pin = new Pin
                {
                    Location = new Location(loc.lat, loc.lon),
                    Label = "Stop"
                };
                DeliveryMap.Pins.Add(pin);
            }


            DeliveryMap.MapElements.Clear();
            DeliveryMap.MapElements.Add(polyline);

            // Optionally move map center
            if (coordinates.Count > 0)
            {
                DeliveryMap.MoveToRegion(MapSpan.FromCenterAndRadius(coordinates[0], Distance.FromKilometers(2)));
            }

            await DisplayAlert("Route Calculated", "Successfully fetched and displayed route!", "OK");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"ORS Directions API error: {error}");
            await DisplayAlert("Error", $"ORS failed: {response.StatusCode}\n{error}", "OK");
        }
    }
}
