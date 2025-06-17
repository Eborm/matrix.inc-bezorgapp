using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezorgapp
{
    public partial class BarcodeScannerPage : ContentPage
    {
        public BarcodeScannerPage()
        {
            InitializeComponent();
        }

        private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var eersteResultaat = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(eersteResultaat))
                return;
            
            Dispatcher.Dispatch(() =>
            {
                barcodeReader.IsDetecting = false;
                
                resultLabel.Text = eersteResultaat;
                
                DisplayAlert("Barcode gescand", resultLabel.Text, "OK");
                
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            barcodeReader.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            barcodeReader.IsDetecting = false;
        }
    }
}