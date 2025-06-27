// App.xaml.cs
// Deze klasse initialiseert de applicatie en stelt het hoofdscherm en thema in.

namespace bezorgapp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Haal de voorkeur voor thema op en stel het thema in
            string theme = Preferences.Get("AppTheme", "System");
            UserAppTheme = theme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified // Systeemthema
            };

            // Stel de hoofdpagina van de app in
            MainPage = new AppShell();
        }
    }
}
