// MainPage.xaml.cs
// Deze klasse definieert de logica voor de hoofdpagina van de app.

using System.Diagnostics;
using bezorgapp.Services;
using System.Globalization;
namespace bezorgapp
{
    public partial class MainPage : ContentPage
    {
        private readonly MapService _mapService;
        public MainPage()
        {
            InitializeComponent();

            _mapService = new MapService();
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
        private async void OnShowMapClicked(object sender, EventArgs e)
        {
            string origin = "";
            var origin_list = await _mapService.GetLocationAsync();
            origin += origin_list[0].ToString(CultureInfo.InvariantCulture);
            origin += ",";
            origin += origin_list[1].ToString(CultureInfo.InvariantCulture);
            Debug.Write(origin);

            // Define coordinates
            string start_end = "50.85056910665486,6.011646432851468"; // Warhouse
            string waypoint1 = "";
            string waypoint2 = "";
            string waypoint3 = ""; 
            List<(double lon, double lat)> cords = new List<(double lon, double lat)>() { _mapService.AdressToCoordinates("123 Elm St")[0], _mapService.AdressToCoordinates("456 Oak St")[0], _mapService.AdressToCoordinates("789 Pine St")[0] };
            var optimizedRoute = await _mapService.OptimizeRouteAsync(cords);
            waypoint1 += optimizedRoute[0].lat.ToString(CultureInfo.InvariantCulture) + "," + optimizedRoute[0].lon.ToString(CultureInfo.InvariantCulture);
            waypoint2 += optimizedRoute[1].lat.ToString(CultureInfo.InvariantCulture) + "," + optimizedRoute[1].lon.ToString(CultureInfo.InvariantCulture);
            waypoint3 += optimizedRoute[2].lat.ToString(CultureInfo.InvariantCulture) + "," + optimizedRoute[2].lon.ToString(CultureInfo.InvariantCulture);


            // Build Google Maps directions URL
            string url = $"https://www.google.com/maps/dir/?api=1&origin={origin}&destination={start_end}&waypoints={start_end}|{waypoint1}|{waypoint2}|{waypoint3}&travelmode=driving";
            Debug.WriteLine(url);
            await Launcher.Default.OpenAsync(url);
        }
    }
}