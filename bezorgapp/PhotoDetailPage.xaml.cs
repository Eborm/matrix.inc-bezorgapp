// PhotoDetailPage.xaml.cs - Code-behind voor het tonen van een foto in detail
namespace bezorgapp;

public partial class PhotoDetailPage : ContentPage
{
    // Constructor ontvangt de URL van de te tonen foto
    public PhotoDetailPage(string imageUrl)
    {
        InitializeComponent(); // Koppel aan de XAML-layout
        DetailImage.Source = imageUrl; // Stel de bron van de afbeelding in
    }
}