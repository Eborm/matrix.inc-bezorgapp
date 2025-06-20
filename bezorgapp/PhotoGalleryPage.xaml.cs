using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;

namespace bezorgapp
{
    public partial class PhotoGalleryPage : ContentPage
    {
        private readonly int _orderId;

        public PhotoGalleryPage(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            Title = $"Foto's voor Order {orderId}";
            LoadImages();
        }

        private async void LoadImages()
        {
            var requestUrl = $"https://bezorgapp-api-1234.azurewebsites.net/api/upload/order/{_orderId}";

            try
            {
                using var httpClient = new HttpClient();
                var imageUrls = await httpClient.GetFromJsonAsync<List<string>>(requestUrl);

                ImagesLayout.Children.Clear();

                if (imageUrls != null && imageUrls.Any())
                {
                    foreach (var url in imageUrls)
                    {
                        var image = new Image
                        {
                            Source = url,
                            HeightRequest = 200,
                            Margin = new Thickness(0, 10)
                        };

                        var tapGesture = new TapGestureRecognizer
                        {
                            Command = new Command(() =>
                            {
                                Navigation.PushAsync(new PhotoDetailPage(url));
                            })
                        };
                        image.GestureRecognizers.Add(tapGesture);

                        ImagesLayout.Children.Add(image);
                    }
                }
                else
                {
                    ImagesLayout.Children.Add(new Label { Text = "Geen foto's gevonden voor deze order.", HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(20) });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", $"Kon foto's niet laden: {ex.Message}", "OK");
            }
        }
    }
}