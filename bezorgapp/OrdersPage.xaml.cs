using bezorgapp.Services;

namespace bezorgapp
{
    public partial class OrdersPage : ContentPage
    {
        private readonly ApiService _apiService;

        public OrdersPage()
        {
            InitializeComponent();
            _apiService = new ApiService(); 
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var orders = await _apiService.GetOrdersAsync();
            foreach (var order in orders)
            {
                order.DeliveryStateState = await _apiService.GetDeliveryStateByIdAsync(order.Id);
            }
            OrdersCollectionView.ItemsSource = orders;
        }
    }
}