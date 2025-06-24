using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using bezorgapp.Services;

namespace bezorgapp;

public partial class CreatePicture : ContentPage
{
    // Het ordernummer waarvoor de foto wordt gemaakt
    private readonly int _orderId;
    // Of de order na uploaden direct als afgeleverd moet worden gemarkeerd
    private readonly bool _markAsCompletedAfterUpload;
    // Service voor communicatie met de backend API
    private readonly ApiService _apiService;
    
    // Constructor ontvangt het ordernummer en optioneel of de order direct afgerond moet worden
    public CreatePicture(int orderId, bool markAsCompletedAfterUpload = false)
    {
        InitializeComponent(); // Koppel aan de XAML-layout
        _orderId = orderId;
        _markAsCompletedAfterUpload = markAsCompletedAfterUpload;
        _apiService = new ApiService();
        
        Title = $"Foto voor Order {_orderId}"; // Zet de paginatitel
    }

    // Wordt aangeroepen als de gebruiker een foto wil maken met de camera
    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            FileResult photo = await MediaPicker.CapturePhotoAsync(); // Open de camera
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                CapturedImage.Source = ImageSource.FromStream(() => stream); // Toon de gemaakte foto
                await UploadAndFinalizeAsync(photo); // Upload de foto en werk eventueel de status bij
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
        }
    }

    // Wordt aangeroepen als de gebruiker een bestaande foto uit de galerij wil kiezen
    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync(); // Open de fotogalerij
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                CapturedImage.Source = ImageSource.FromStream(() => stream); // Toon de gekozen foto
                await UploadAndFinalizeAsync(photo); // Upload de foto en werk eventueel de status bij
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging iets mis: {ex.Message}", "OK");
        }
    }
    
    // Upload de foto naar de server en werk eventueel de orderstatus bij
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
            var uploadResponse = await httpClient.PostAsync(uploadUrl, content); // Upload de foto

            if (!uploadResponse.IsSuccessStatusCode)
            {
                string error = await uploadResponse.Content.ReadAsStringAsync();
                await DisplayAlert("Fout bij upload", $"Upload mislukt: {uploadResponse.StatusCode}\n{error}", "OK");
                return;
            }
            
            // Indien gewenst, markeer de order direct als afgeleverd
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
            await Navigation.PopAsync(); // Ga terug naar de vorige pagina
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fout", $"Er ging een onverwachte fout op: {ex.Message}", "OK");
        }
    }
    
    // Wordt aangeroepen als de gebruiker de galerij van deze order wil bekijken
    private async void OnShowGalleryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PhotoGalleryPage(_orderId));
    }
}