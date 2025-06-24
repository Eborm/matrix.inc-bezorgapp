using bezorgapp.Services;
using bezorgapp.Models;
using System.Linq;

namespace bezorgapp
{
    public partial class OrdersPage : ContentPage
    {
        // Service voor communicatie met de backend API
        private readonly ApiService _apiService;

        public OrdersPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        // Wanneer de pagina verschijnt, automatisch de orders laden
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadOrdersAsync();
        }

        // Haal de orders op en toon ze in de CollectionView
        private async Task LoadOrdersAsync()
        {
            loadingIndicator.IsVisible = true; // Laat laadindicator zien
            OrdersCollectionView.IsVisible = false; // Verberg de lijst tijdelijk

            try
            {
                var filteredOrders = await _apiService.GetOrdersAsync(); // Haal orders op via de API
                OrdersCollectionView.ItemsSource = filteredOrders; // Toon de orders in de lijst
            }
            finally
            {
                loadingIndicator.IsVisible = false; // Verberg laadindicator
                OrdersCollectionView.IsVisible = true; // Toon de lijst weer
            }
        }

        // Als de gebruiker op de ververs-knop drukt, herlaad de pagina
        public async void OnRefreshClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage());
        }

        // Wanneer een order wordt aangetikt, laat een actiemenu zien
        private async void OnOrderTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not Order selectedOrder)
                return; // Geen geldige order geselecteerd

            // Laat een actiemenu zien met opties voor de geselecteerde order
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
                    // Zet de status van de order op 'Onderweg'
                    result = await _apiService.MarkAsInProgressAsync(selectedOrder.Id);
                    actionChosen = true;
                    break;

                case "Markeer als Afgeleverd":
                    // Alleen mogelijk als de order al 'Onderweg' is
                    if (selectedOrder.DeliveryState == "Onderweg")
                    {
                        await Navigation.PushAsync(new CreatePicture(selectedOrder.Id, true)); // Foto maken als bewijs
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
                    // Toon de fotogalerij voor deze order
                    await Navigation.PushAsync(new PhotoGalleryPage(selectedOrder.Id));
                    loadingIndicator.IsVisible = false;
                    return;
            }

            // Toon een melding afhankelijk van het resultaat van de gekozen actie
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
            
            // Herlaad de orders als er een actie is uitgevoerd, anders verberg de laadindicator
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