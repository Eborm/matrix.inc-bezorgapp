// SettingsPage.xaml.cs - Code-behind voor de instellingenpagina
namespace bezorgapp
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent(); // Koppel aan de XAML-layout
            // Haal de huidige dark mode-instelling op en zet de schakelaar in de juiste stand
            bool isDarkMode = Preferences.Get("DarkModeEnabled", false);
            ThemeSwitch.IsToggled = isDarkMode;
        }
        
        // Wordt aangeroepen als de gebruiker de dark mode-schakelaar omzet
        private void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            if (Application.Current != null)
            {
                // Pas het thema van de app direct aan
                Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
                // Sla de voorkeur van de gebruiker op
                Preferences.Set("DarkModeEnabled", e.Value);
            }
        }
    }
}