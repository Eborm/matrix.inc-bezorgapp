// App.xaml.cs
// Deze klasse initialiseert de applicatie en stelt het hoofdscherm en thema in.

namespace bezorgapp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Haal de voorkeur voor donkere modus op en stel het thema in
            bool isDarkMode = Preferences.Get("DarkModeEnabled", false);
            UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

            // Stel de hoofdpagina van de app in
            MainPage = new AppShell();
        }
    }
}
