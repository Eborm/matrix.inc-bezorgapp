using bezorgapp.Services;
using System.Linq;

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
            
            var filteredOrders = orders.Where(o => o.DeliveryServiceName == "Tempnaam").ToList();

            foreach (var order in filteredOrders)
            {
                order.DeliveryStateState = await _apiService.GetDeliveryStateByIdAsync(order.Id);

                if (order.DeliveryStateState == 0)
                {
                    order.DeliveryState = "In afwachting";
                }
                else if (order.DeliveryStateState == 1)
                {
                    order.DeliveryState = "Onderweg";
                }
                else if (order.DeliveryStateState == 2)
                {
                    order.DeliveryState = "Afgeleverd";
                }
                else
                {
                    order.DeliveryState = "Onbekend";
                }
            }
            OrdersCollectionView.ItemsSource = filteredOrders;
        }
    }
}