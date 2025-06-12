namespace bezorgapp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
        private void Oncounterunclicked(object sender, EventArgs e)
        {
            count--;
            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";
            SemanticScreenReader.Announce(CounterBtn.Text);
        }
        
        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                FileResult photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();
                    CapturedImage.Source = ImageSource.FromStream(() => stream);

                    // Upload hier
                    await UploadPhotoAsync(photo);
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Fout", "Camera wordt niet ondersteund op dit apparaat.", "OK");
            }
            catch (PermissionException)
            {
                await DisplayAlert("Fout", "Geen toestemming voor camera.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
            }
        }
        
        private async Task UploadPhotoAsync(FileResult photo)
        {
            try
            {
                if (photo == null)
                    return;

                using var stream = await photo.OpenReadAsync();
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "file", photo.FileName);

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("https://bezorgapp-api-1234.azurewebsites.net/api/upload/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Succes", "Foto geüpload!", "OK");
                }
                else
                {
                    await DisplayAlert("Fout", $"Upload mislukt: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
            }
        }
        
        private async void OnShowGalleryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PhotoGalleryPage());
        }
    }

}
