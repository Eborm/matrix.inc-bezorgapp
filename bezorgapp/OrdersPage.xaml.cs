using bezorgapp.Services;
using bezorgapp.Models;
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
                var filteredOrders = await _apiService.GetOrdersAsync();
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
        $"Opties voor Order {selectedOrder.Id}",
        "Annuleren",
        null,
        "Markeer als Onderweg", 
        "Markeer als Afgeleverd",
        "Bekijk foto's");

    (bool success, string errorMessage) result = (false, "Geen actie gekozen.");
    bool actionChosen = false;

    loadingIndicator.IsVisible = true;

    switch (action)
    {
        case "Markeer als Onderweg":
            result = await _apiService.MarkAsInProgressAsync(selectedOrder.Id);
            actionChosen = true;
            break;

        case "Markeer als Afgeleverd":
            if (selectedOrder.DeliveryState == "Onderweg")
            {
                await Navigation.PushAsync(new CreatePicture(selectedOrder.Id, true));
                loadingIndicator.IsVisible = false;
                return; 
            }
            else
            {
                await DisplayAlert("Actie niet toegestaan", "Een order kan alleen als afgeleverd worden gemarkeerd als de status 'Onderweg' is.", "OK");
                actionChosen = false;
            }
            break;

        case "Bekijk foto's":
            await Navigation.PushAsync(new PhotoGalleryPage(selectedOrder.Id));
            loadingIndicator.IsVisible = false;
            return;
    }

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
    
    if (actionChosen)
    {
        await LoadOrdersAsync();
    }
    else
    {
        loadingIndicator.IsVisible = false;
    }
}
    }
}