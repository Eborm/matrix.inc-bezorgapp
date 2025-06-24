// MainPage.xaml.cs
// Deze klasse definieert de logica voor de hoofdpagina van de app.

namespace bezorgapp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        // Handler voor de knop 'Bekijk Orders'
        private async void OnBekijkOrdersClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage());
        }
        
        // Handler voor de actieknop (barcode scannen)
        private void OnActionButtonClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BarcodeScannerPage());
        }
    }
}