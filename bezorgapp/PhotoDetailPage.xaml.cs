namespace bezorgapp;

public partial class PhotoDetailPage : ContentPage
{
    public PhotoDetailPage(string imageUrl)
    {
        InitializeComponent();
        DetailImage.Source = imageUrl;
    }
}