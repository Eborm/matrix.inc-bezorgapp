using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace bezorgapp.Services
{
    class MapService
    {
        private readonly string orsApiKey = "5b3ce3597851110001cf6248969d1894c7d94094bab990a29141afca";
        private readonly Dictionary<string, string> AdressesToCords = new Dictionary<string, string>
        {
            { "warhouse",    "50.85056910665486, 6.011646432851468" },
            { "123 Elm St",  "50.892543417163814, 5.98174669784661" },
            { "456 Oak St",  "50.882447992696584, 5.98098648461065" },
            { "789 Pine St", "50.87175367817815, 5.998289390834275" }
        };
        public async Task<List<(double lon, double lat)>> OptimizeRouteAsync(List<(double lon, double lat)> locations)
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
                        }
                    }

                    return OptimezedRoute; // <--- This returns the reordered list!
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"ORS error response: {error}");
                return locations; // Return the original order if optimization fails
            }
        }

        public List<(double lon, double lat)> AdressToCoordinates(string address)
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

        public async Task<List<double>> GetLocationAsync()
        {
            Location location = await Geolocation.Default.GetLastKnownLocationAsync();
            List<double> location_return = new List<double> { location.Latitude, location.Longitude };
            return location_return;
        }
    }
}
