using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;

namespace bezorgapp
{
    public partial class PhotoGalleryPage : ContentPage
    {
        private const string ListEndpoint = "https://bezorgapp-api-1234.azurewebsites.net/api/upload/list";

        public PhotoGalleryPage()
        {
            InitializeComponent();
            LoadImages();
        }

        private async void LoadImages()
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageUrls = await httpClient.GetFromJsonAsync<List<string>>(ListEndpoint);

                if (imageUrls != null)
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
                    await DisplayAlert("Info", "Geen foto's gevonden.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", $"Kon foto's niet laden: {ex.Message}", "OK");
            }
        }
    }
}