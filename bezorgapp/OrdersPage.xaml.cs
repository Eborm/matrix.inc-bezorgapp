using bezorgapp.Models;
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
            await LoadOrdersAsync();
        }
        
        private async Task LoadOrdersAsync()
        {
            loadingIndicator.IsVisible = true;
            OrdersCollectionView.IsVisible = false;

            try
            {
                var allOrders = await _apiService.GetOrdersAsync();
                var filteredOrders = allOrders.Where(o => o.DeliveryServiceName == "Tempnaam").ToList();

                foreach (var order in filteredOrders)
                {
                    switch (order.DeliveryStateState)
                    {
                        case 0:
                            order.DeliveryState = "In afwachting";
                            break;
                        case 1:
                            order.DeliveryState = "Onderweg";
                            break;
                        case 2:
                            order.DeliveryState = "Afgeleverd";
                            break;
                        default:
                            order.DeliveryState = "Onbekend";
                            break;
                    }
                }
                
                OrdersCollectionView.ItemsSource = filteredOrders;
            }
            finally
            {
                loadingIndicator.IsVisible = false;
                OrdersCollectionView.IsVisible = true;
            }
        }
        
        private async void OnOrderTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not Order selectedOrder)
                return;

            string action = await DisplayActionSheet(
                $"Order {selectedOrder.Id} status wijzigen",
                "Annuleren",
                null,
                "Markeer als Onderweg", "Markeer als Afgeleverd");

            (bool success, string errorMessage) result = (false, "Geen actie gekozen.");
            bool actionChosen = false;

            loadingIndicator.IsVisible = true;

            if (action == "Markeer als Onderweg")
            {
                // Roep de (geschatte) methode voor 'Onderweg' aan
                result = await _apiService.MarkAsInProgressAsync(selectedOrder.Id);
                actionChosen = true;
            }
            else if (action == "Markeer als Afgeleverd")
            {
                // Roep de correcte methode voor 'Afgeleverd' aan
                result = await _apiService.MarkAsCompletedAsync(selectedOrder.Id);
                actionChosen = true;
            }

            // Als er een actie is gekozen, verwerk het resultaat
            if (actionChosen)
            {
                if (result.success)
                {
                    await DisplayAlert("Succes", "De status is bijgewerkt.", "OK");
                }
                else
                {
                    await DisplayAlert("Fout", $"De status kon niet worden bijgewerkt:\n\n{result.errorMessage}", "OK");
                }
            }
        
            // Herlaad de lijst om de wijziging te zien (of als er niets is gekozen)
            await LoadOrdersAsync();
            loadingIndicator.IsVisible = false;
        }
    }
}