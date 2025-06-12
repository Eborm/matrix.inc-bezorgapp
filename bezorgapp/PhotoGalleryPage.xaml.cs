using System.Net.Http.Json;
using System.Text.Json;

namespace bezorgapp
{
    public partial class PhotoGalleryPage : ContentPage
    {
        private const string BaseUrl = "https://bezorgapp-api-1234.azurewebsites.net";

        public PhotoGalleryPage()
        {
            InitializeComponent();
            LoadPhotos();
        }

        private async void LoadPhotos()
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{BaseUrl}/api/upload/list");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var fileNames = JsonSerializer.Deserialize<List<string>>(json);

                    if (fileNames != null)
                    {
                        foreach (var imageUrl in fileNames)
                        {
                            var image = new Image
                            {
                                Source = ImageSource.FromUri(new Uri(imageUrl)),
                                HeightRequest = 200,
                                Margin = new Thickness(0, 0, 0, 20)
                            };
                            ImagesLayout.Children.Add(image);
                        }
                        
                    }
                }
                else
                {
                    await DisplayAlert("Fout", $"Kon foto's niet laden: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
            }
        }
    }
}