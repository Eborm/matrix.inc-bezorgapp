using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using bezorgapp.Services;

namespace bezorgapp;

public partial class CreatePicture : ContentPage
{
    private readonly int _orderId;
    private readonly bool _markAsCompletedAfterUpload;
    private readonly ApiService _apiService;
    
    public CreatePicture(int orderId, bool markAsCompletedAfterUpload = false)
    {
        InitializeComponent();
        _orderId = orderId;
        _markAsCompletedAfterUpload = markAsCompletedAfterUpload;
        _apiService = new ApiService();
        
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
                await UploadAndFinalizeAsync(photo);
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
                await UploadAndFinalizeAsync(photo); // Aangepaste methode aanroepen
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
        }
    }
    
    private async Task UploadAndFinalizeAsync(FileResult photo)
    {
        if (photo == null) return;

        try
        {
            var uploadUrl = $"{_apiService.BaseUrl}/api/upload/order/{_orderId}";

            using var stream = await photo.OpenReadAsync();
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", photo.FileName);

            using var httpClient = new HttpClient();
            var uploadResponse = await httpClient.PostAsync(uploadUrl, content);

            if (!uploadResponse.IsSuccessStatusCode)
            {
                string error = await uploadResponse.Content.ReadAsStringAsync();
                await DisplayAlert("Fout bij upload", $"Upload mislukt: {uploadResponse.StatusCode}\n{error}", "OK");
                return;
            }
            
            if (_markAsCompletedAfterUpload)
            {
                var (success, errorMessage) = await _apiService.MarkAsCompletedAsync(_orderId);
                if (!success)
                {
                    await DisplayAlert("Fout bij statusupdate", $"Foto is geüpload, maar de status kon niet worden bijgewerkt:\n\n{errorMessage}", "OK");
                    return;
                }
            }
            
            await DisplayAlert("Succes", "Order succesvol afgeleverd en foto opgeslagen.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging een onverwachte fout op: {ex.Message}", "OK");
        }
    }
    
    private async void OnShowGalleryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PhotoGalleryPage(_orderId));
    }
}