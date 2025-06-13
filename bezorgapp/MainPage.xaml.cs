namespace bezorgapp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            bool isDarkMode = Preferences.Get("DarkModeEnabled", false);
            ThemeSwitch.IsToggled = isDarkMode;
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
            }
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
        
        private void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
                Preferences.Set("DarkModeEnabled", e.Value);
            }
        }
        
        private async void OnPickPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();
                    CapturedImage.Source = ImageSource.FromStream(() => stream);
            
                    await UploadPhotoAsync(photo);
                }
            }
            catch (PermissionException)
            {
                await DisplayAlert("Fout", "Geen toestemming voor toegang tot de fotogalerij.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
            }
        }

    }
}
