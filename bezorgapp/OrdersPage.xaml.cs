using System.Collections.ObjectModel;
using bezorgapp.Services;
using bezorgapp.Models;
using System.Linq;

namespace bezorgapp
{
    public partial class OrdersPage : ContentPage
    {
        // Model voor sorteeropties (alleen nodig op deze pagina)
        public class Sortation
        {
            public string SortName { get; set; }
            public Func<Order, object> SortFunc { get; set; }
        }

        // Service voor communicatie met de backend API
        private readonly ApiService _apiService;

        // Opties voor sortering van orders
        private List<Sortation> _sortOptions;
        
        // Huidig geselecteerde sorteeroptie
        public Sortation SelectedSortOption { get; set; }

        public OrdersPage()
        {
            InitializeComponent();
            
            // Initializeer API service
            _apiService = new ApiService();
            
            // Vul sorteeropties met gewenste opties
            _sortOptions = new List<Sortation>
            {
                new Sortation { SortName = "Delivery State", SortFunc = o => o.DeliveryState },
                new Sortation { SortName = "Customer Name", SortFunc = o => o.Customer.Name },
                new Sortation { SortName = "Orer ID", SortFunc = o => o.Id }
            };

            // Bind sorteringsopties aan picker in pagina
            SortOption.ItemsSource = _sortOptions;
            
            // Zet een standaard sorteeroptie
            SelectedSortOption = _sortOptions[0]; // Delivery State
            SortOption.SelectedItem = SelectedSortOption; // Zet standaard ook op pagina's picker
        }

        // Wanneer de pagina verschijnt, automatisch de orders laden
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadOrdersAsync(SelectedSortOption.SortFunc);
        }

        // Haal de orders op en toon ze in de CollectionView
        private async Task LoadOrdersAsync(Func<Order, object>? sortation=null)
        {
            loadingIndicator.IsVisible = true; // Laat laadindicator zien
            OrdersCollectionView.IsVisible = false; // Verberg de lijst tijdelijk

            try
            {
                var orders = await _apiService.GetOrdersAsync(); // Haal orders op via de API
                
                // Kijk of een sorteringsfunctie was meegegeven
                // zo ja, pas deze toe op de lijst orders
                if (sortation != null)
                {
                    orders = orders.OrderBy(sortation).ToList();
                }
                
                
                OrdersCollectionView.ItemsSource = orders; // Toon de orders in de lijst
            }
            finally
            {
                loadingIndicator.IsVisible = false; // Verberg laadindicator
                OrdersCollectionView.IsVisible = true; // Toon de lijst weer
            }
        }

        // Als de gebruiker op de refresh-knop drukt, herlaad de data
        public async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadOrdersAsync();
        }
        
        public async void OnSortationIndexChanged(object sender, EventArgs args)
        {
            if (SortOption.SelectedItem is Sortation selected)
            {
                SelectedSortOption = selected;
                await LoadOrdersAsync(selected.SortFunc);
            }
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

            (bool success, string? errorMessage) result = (false, "Geen actie gekozen.");
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
