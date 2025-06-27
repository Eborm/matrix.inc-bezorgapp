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
        
        private async void OnBekijkOrdersClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage());
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
            string waypoint1 = "50.892543417163814,5.98174669784661";
            string waypoint2 = "50.882447992696584,5.98098648461065";

            // Build Google Maps directions URL
            string url = $"https://www.google.com/maps/dir/?api=1&origin={origin}&destination={start_end}&waypoints={start_end}|{waypoint1}|{waypoint2}&travelmode=driving";

            await Launcher.Default.OpenAsync(url);
        }
    }
}