namespace bezorgapp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnApiTestButtonClicked(object sender, EventArgs e)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                string apiKey = "7a38a102-e061-4679-9919-ea47586d7fa3";
                
                httpClient.DefaultRequestHeaders.Add("apiKey", apiKey);

                string apiUrl = "http://51.137.100.120:5000/api/DeliveryServices/IsAuthenticationOk";

                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Succes!", $"De API is bereikbaar en de key is geaccepteerd!\n\nAntwoord: {content}", "OK");
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Fout", $"De API gaf een fout terug.\nStatus Code: {response.StatusCode}\n\nDetails: {errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Verbindingsfout", $"Kon geen verbinding maken met de API.\n\nDetails: {ex.Message}", "OK");
            }
        }
        
        private async void OnBekijkOrdersClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage());
        }
    }
}