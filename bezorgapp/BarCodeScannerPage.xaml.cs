using System;
using System.Linq;
using System.Threading.Tasks;
using bezorgapp.Services;
using bezorgapp.Models;

namespace bezorgapp
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private readonly ApiService _apiService;

        public BarcodeScannerPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }
        
        private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var eersteResultaat = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(eersteResultaat))
                return;
            
            barcodeReader.IsDetecting = false;
            
            Dispatcher.Dispatch(async () =>
            {
                loadingIndicator.IsVisible = true;

                if (!int.TryParse(eersteResultaat, out int scannedOrderId))
                {
                    await DisplayAlert("Fout", "De gescande barcode is geen geldig ordernummer.", "OK");
                    barcodeReader.IsDetecting = true;
                    loadingIndicator.IsVisible = false;
                    return;
                }
                
                var foundOrder = await _apiService.GetOrderByIdAsync(scannedOrderId);

                if (foundOrder == null)
                {
                    await DisplayAlert("Niet gevonden", $"Order {scannedOrderId} is niet gevonden.", "OK");
                    barcodeReader.IsDetecting = true;
                    loadingIndicator.IsVisible = false;
                    return;
                }

                switch (foundOrder.DeliveryState)
                {
                    case "In afwachting":
                        var (inProgressSuccess, inProgressError) = await _apiService.MarkAsInProgressAsync(foundOrder.Id);
                        if (inProgressSuccess)
                        {
                            await DisplayAlert("Succes", $"Order {foundOrder.Id} is nu 'Onderweg'.", "OK");
                            await Navigation.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Fout", $"Kon status niet bijwerken: {inProgressError}", "OK");
                        }
                        break;

                    case "Onderweg":
                        await DisplayAlert("Info", "Order is al onderweg. Maak een foto om de aflevering te voltooien.", "OK");
                        await Navigation.PushAsync(new CreatePicture(foundOrder.Id, true));
                        break;

                    default:
                        await DisplayAlert("Info", $"Er is geen automatische actie voor order {foundOrder.Id} met status '{foundOrder.DeliveryState}'.", "OK");
                        barcodeReader.IsDetecting = true;
                        break;
                }

                loadingIndicator.IsVisible = false;
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Dispatcher.Dispatch(() =>
            {
                barcodeReader.IsDetecting = true;
                loadingIndicator.IsVisible = false;
            });
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            barcodeReader.IsDetecting = false;
        }
    }
}