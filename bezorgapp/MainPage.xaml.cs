namespace bezorgapp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        private async void OnBekijkOrdersClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage());
        }
        
        private void OnActionButtonClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BarcodeScannerPage());
        }
    }
}