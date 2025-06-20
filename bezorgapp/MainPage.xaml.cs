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
        private async void OnShowMapClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(MapPage));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex}");
                await DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }
    }
}