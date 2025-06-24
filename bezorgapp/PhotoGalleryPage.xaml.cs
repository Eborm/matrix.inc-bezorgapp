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
        // Het ordernummer waarvoor de foto's worden opgehaald
        private readonly int _orderId;

        // Constructor ontvangt het ordernummer
        public PhotoGalleryPage(int orderId)
        {
            InitializeComponent(); // Koppel aan de XAML-layout
            _orderId = orderId;
            Title = $"Foto's voor Order {orderId}"; // Zet de paginatitel
            LoadImages(); // Laad direct de foto's bij het openen
        }

        // Haal de foto's op van de server en toon ze in de FlexLayout
        private async void LoadImages()
        {
            var requestUrl = $"https://bezorgapp-api-1234.azurewebsites.net/api/upload/order/{_orderId}";

            try
            {
                using var httpClient = new HttpClient();
                var imageUrls = await httpClient.GetFromJsonAsync<List<string>>(requestUrl); // Haal lijst met foto-urls op

                ImagesLayout.Children.Clear(); // Maak de layout eerst leeg

                if (imageUrls != null && imageUrls.Any())
                {
                    // Voeg elke foto toe als een Image met klikfunctionaliteit
                    foreach (var url in imageUrls)
                    {
                        var image = new Image
                        {
                            Source = url,
                            Margin = new Thickness(5),
                            HeightRequest = 120, 
                            WidthRequest = 120,
                            Aspect = Aspect.AspectFill
                        };

                        // Voeg een tap-gesture toe om de foto in detail te bekijken
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
                    // Geen foto's gevonden voor deze order
                    ImagesLayout.Children.Add(new Label { Text = "Geen foto's gevonden voor deze order.", HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(20) });
                }
            }
            catch (Exception ex)
            {
                // Foutmelding als het ophalen mislukt
                await DisplayAlert("Fout", $"Kon foto's niet laden: {ex.Message}", "OK");
            }
        }
    }
}