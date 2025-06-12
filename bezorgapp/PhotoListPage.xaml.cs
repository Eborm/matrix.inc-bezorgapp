using System.Net.Http.Json;

namespace bezorgapp
{
    public partial class PhotoListPage : ContentPage
    {
        public PhotoListPage()
        {
            InitializeComponent();
            LoadPhotosAsync();
        }

        private async void OnViewPhotosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PhotoListPage());
        }

        private async void LoadPhotosAsync()
        {
            try
            {
                using var client = new HttpClient();
                var photos = await client.GetFromJsonAsync<List<string>>("https://bezorgapp-api-1234.azurewebsites.net/api/upload/list");

                if (photos != null)
                {
                    foreach (var url in photos)
                    {
                        Image image = new Image
                        {
                            Source = ImageSource.FromUri(new Uri(url)),
                            HeightRequest = 200
                        };
                        ImageList.Children.Add(image);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", $"Kan foto's niet laden: {ex.Message}", "OK");
            }
        }
    }
}