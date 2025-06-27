// BarCodeScannerPage.xaml.cs - Pagina voor het scannen van barcodes en verwerken van orders
using System;
using System.Linq;
using System.Threading.Tasks;
using bezorgapp.Services;
using bezorgapp.Models;

namespace bezorgapp
{
    public partial class BarcodeScannerPage : ContentPage
    {
        // Service voor communicatie met de backend API
        private readonly ApiService _apiService;

        // Constructor: initialiseert de pagina en de ApiService
        public BarcodeScannerPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }
        
        // Wordt aangeroepen als er een barcode wordt gedetecteerd
        private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var eersteResultaat = e.Results?.FirstOrDefault()?.Value; // Pak het eerste resultaat
            if (string.IsNullOrEmpty(eersteResultaat))
                return;
            
            barcodeReader.IsDetecting = false; // Stop met scannen tijdens verwerking
            
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
                
                // Haal de order op uit de backend
                var foundOrder = await _apiService.GetOrderByIdAsync(scannedOrderId);

                if (foundOrder == null)
                {
                    await DisplayAlert("Niet gevonden", $"Order {scannedOrderId} is niet gevonden.", "OK");
                    barcodeReader.IsDetecting = true;
                    loadingIndicator.IsVisible = false;
                    return;
                }

                // Voer een actie uit afhankelijk van de status van de order
                switch (foundOrder.DeliveryState)
                {
                    case "In afwachting":
                        // Markeer de order als 'Onderweg'
                        var (inProgressSuccess, inProgressError) = await _apiService.MarkAsInProgressAsync(foundOrder.Id);
                        if (inProgressSuccess)
                        {
                            await DisplayAlert("Succes", $"Order {foundOrder.Id} is nu 'Onderweg'.", "OK");
                            await Navigation.PopAsync(); // Ga terug naar vorige pagina
                        }
                        else
                        {
                            await DisplayAlert("Fout", $"Kon status niet bijwerken: {inProgressError}", "OK");
                        }
                        break;

                    case "Onderweg":
                        // Order is al onderweg, start het proces om een afleverfoto te maken
                        await DisplayAlert("Info", "Order is al onderweg. Maak een foto om de aflevering te voltooien.", "OK");
                        await Navigation.PushAsync(new CreatePicture(foundOrder.Id, true));
                        break;

                    default:
                        // Geen automatische actie voor deze status
                        await DisplayAlert("Info", $"Er is geen automatische actie voor order {foundOrder.Id} met status '{foundOrder.DeliveryState}'.", "OK");
                        barcodeReader.IsDetecting = true;
                        break;
                }

                loadingIndicator.IsVisible = false; // Verberg laadindicator
            });
        }

        // Wordt aangeroepen als de pagina verschijnt
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Dispatcher.Dispatch(() =>
            {
                barcodeReader.IsDetecting = true; // Start met scannen
                loadingIndicator.IsVisible = false; // Verberg laadindicator
            });
        }
        
        // Wordt aangeroepen als de pagina verdwijnt
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            barcodeReader.IsDetecting = false; // Stop met scannen
        }
    }
}