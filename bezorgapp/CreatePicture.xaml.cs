using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace bezorgapp;

public partial class CreatePicture : ContentPage
{
    private readonly int _orderId;
    
    public CreatePicture(int orderId)
    {
        InitializeComponent();
        _orderId = orderId;
        Title = $"Foto voor Order {_orderId}";
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
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
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
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
        }
    }
    
    private async Task UploadPhotoAsync(FileResult photo)
    {
        try
        {
            if (photo == null) return;
            
            var uploadUrl = $"https://bezorgapp-api-1234.azurewebsites.net/api/upload/order/{_orderId}";

            using var stream = await photo.OpenReadAsync();
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", photo.FileName);

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(uploadUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Succes", "Foto geüpload!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Fout", $"Upload mislukt: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
        }
    }

    private async void OnShowGalleryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PhotoGalleryPage(_orderId));
    }
}