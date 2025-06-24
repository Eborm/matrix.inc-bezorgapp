using System;
using System.Linq;
using System.Threading.Tasks;
using bezorgapp.Services;
using bezorgapp.Models;
using Plugin.Firebase.Firestore;

namespace bezorgapp
{
    public partial class BarcodeScannerPage : ContentPage
    {
        // Service voor communicatie met de backend API
        private readonly ApiService _apiService;

        public BarcodeScannerPage()
        {
            InitializeComponent(); // Koppel aan de XAML-layout
            _apiService = new ApiService();
        }

        // Wordt aangeroepen als er barcodes zijn gedetecteerd
        private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var eersteResultaat = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(eersteResultaat))
                return; // Geen barcode gevonden
            
            barcodeReader.IsDetecting = false; // Stop met scannen
            
            Dispatcher.Dispatch(async () =>
            {
                loadingIndicator.IsVisible = true; // Laat laadindicator zien

                // Controleer of de barcode een geldig ordernummer is
                if (!int.TryParse(eersteResultaat, out int scannedOrderId))
                {
                    await DisplayAlert("Fout", "De gescande barcode is geen geldig ordernummer.", "OK");
                    barcodeReader.IsDetecting = true;
                    loadingIndicator.IsVisible = false;
                    return;
                }

                // Haal de orders van de gebruiker op
                var userOrders = await _apiService.GetOrdersAsync();
                var foundOrder = userOrders.FirstOrDefault(o => o.Id == scannedOrderId);

                if (foundOrder == null)
                {
                    await DisplayAlert("Niet gevonden", $"Order {scannedOrderId} is niet gevonden in jouw lijst.", "OK");
                    barcodeReader.IsDetecting = true;
                    loadingIndicator.IsVisible = false;
                    return;
                }

                // Bepaal de volgende actie op basis van de status van de order
                switch (foundOrder.DeliveryState)
                {
                    case "In afwachting":
                        // Zet de order op 'Onderweg'
                        var (inProgressSuccess, inProgressError) = await _apiService.MarkAsInProgressAsync(foundOrder.Id);
                        if (inProgressSuccess)
                        {
                            await SaveScanToFirestore(eersteResultaat); // <-- Firestore opslag
                            await DisplayAlert("Succes", $"Order {foundOrder.Id} is nu 'Onderweg'.", "OK");
                            await Navigation.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Fout", $"Kon status niet bijwerken: {inProgressError}", "OK");
                        }
                        break;

                    case "Onderweg":
                        // Order is al onderweg, vraag om een foto te maken
                        await SaveScanToFirestore(eersteResultaat); // <-- Firestore opslag
                        await DisplayAlert("Info", "Order is al onderweg. Maak een foto om de aflevering te voltooien.", "OK");
                        await Navigation.PushAsync(new CreatePicture(foundOrder.Id, true));
                        break;

                    default:
                        // Geen automatische actie voor deze status
                        await SaveScanToFirestore(eersteResultaat); // <-- Firestore opslag
                        await DisplayAlert("Info", $"Er is geen automatische actie voor order {foundOrder.Id} met status '{foundOrder.DeliveryState}'.", "OK");
                        barcodeReader.IsDetecting = true;
                        break;
                }

                loadingIndicator.IsVisible = false; // Verberg laadindicator
            });
        }

        // Functie om scan op te slaan in Firestore
        private async Task SaveScanToFirestore(string barcode)
        {
            var scanData = new
            {
                barcode = barcode,
                timestamp = DateTime.UtcNow
            };

            await CrossFirebaseFirestore.Current
                .GetCollection("scans")
                .AddDocumentAsync(scanData);
        }

        // Start de barcode scanner als de pagina verschijnt
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Dispatcher.Dispatch(() =>
            {
                barcodeReader.IsDetecting = true;
                loadingIndicator.IsVisible = false;
            });
        }

        // Stop de barcode scanner als de pagina verdwijnt
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            barcodeReader.IsDetecting = false;
        }
    }
}