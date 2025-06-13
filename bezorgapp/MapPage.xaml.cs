using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace bezorgapp;

public partial class MapPage : ContentPage
{
    private readonly string orsApiKey = "5b3ce3597851110001cf6248969d1894c7d94094bab990a29141afca";
    private readonly string bezorgappApiKey = " 7a38a102-e061-4679-9919-ea47586d7fa3";
    private readonly string bezorgappApiURL = "http://51.137.100.120:5000/api/";
    private readonly Dictionary<string, string> deliveryLocations = new Dictionary<string, string>
    {
        { "123 Elm St", "40.93387193082394, -73.89109905918852" },
        { "456 Oak St", "40.92778544896371, -73.89090211231711" },
        { "789 Pine St", "40.66591172473292, -73.86833455372528" } 
    };
    public MapPage()
    {
        InitializeComponent();
        GetDeleverys();
    }

    private async Task GetDeleverys()
    {
        CompleteAllOldOrdersAsync();
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("ApiKey", bezorgappApiKey);
        try
        {
            var response = await httpClient.GetAsync("http://51.137.100.120:5000/api/Order");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                await DisplayAlert("Antwoord", jsonResponse, "W");
            }
            else
            {
                await DisplayAlert("Fout", "Kon de leveringen niet ophalen.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
        }
    }

    private async Task CompleteAllOldOrdersAsync()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("ApiKey", bezorgappApiKey);

        try
        {
            // 1. Haal alle orders op
            var orderResponse = await httpClient.GetAsync($"{bezorgappApiURL}Order");
            if (!orderResponse.IsSuccessStatusCode)
            {
                await DisplayAlert("Fout", "Kon orders niet ophalen.", "OK");
                return;
            }

            var orderJson = await orderResponse.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<List<Order>>(orderJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // 2. Haal alle delivery states op
            var deliveryResponse = await httpClient.GetAsync($"{bezorgappApiURL}DeliveryStates/GetAllDeliveryStates");
            if (!deliveryResponse.IsSuccessStatusCode)
            {
                await DisplayAlert("Fout", "Kon delivery states niet ophalen.", "OK");
                return;
            }

            var deliveryJson = await deliveryResponse.Content.ReadAsStringAsync();
            var deliveryStates = JsonSerializer.Deserialize<List<DeliveryState>>(deliveryJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (orders == null || deliveryStates == null)
            {
                await DisplayAlert("Fout", "Data kon niet verwerkt worden.", "OK");
                return;
            }

            var today = DateTime.Today;
            int deliveredCount = 0;

            foreach (var order in orders)
            {
                // Alleen orders van vóór vandaag
                if (order.OrderDate.Date >= today)
                    continue;

                var statesForOrder = deliveryStates.Where(ds => ds.OrderId == order.Id).ToList();

                bool isAlreadyDelivered = statesForOrder.Any(ds => ds.State == 3);

                if (!isAlreadyDelivered)
                {
                    await httpClient.GetAsync($"{bezorgappApiURL}DeliveryStates/StartDelivery?OrderId={order.Id}");
                    await httpClient.GetAsync($"{bezorgappApiURL}DeliveryStates/CompleteDelivery?OrderId={order.Id}");
                    deliveredCount++;
                }
            }

            await DisplayAlert("Klaar", $"{deliveredCount} oude orders zijn nu afgeleverd.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
        }
    }




    private async Task OptimizeRouteAsync()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", orsApiKey);
    }
}