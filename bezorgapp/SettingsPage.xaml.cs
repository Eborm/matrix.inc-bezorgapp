// SettingsPage.xaml.cs - Code-behind voor de instellingenpagina
namespace bezorgapp
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent(); // Koppel aan de XAML-layout
            // Haal de huidige thema-instelling op, standaard 'System'
            string theme = Preferences.Get("AppTheme", "System");
            int selectedIndex = theme switch
            {
                "Light" => 1,
                "Dark" => 2,
                _ => 0 // "System"
            };
            ThemePicker.SelectedIndex = selectedIndex;
            // Pas het thema direct toe bij openen
            ApplyTheme(theme);
        }
        
        // Wordt aangeroepen als de gebruiker de thema-picker wijzigt
        private void OnThemePickerChanged(object sender, EventArgs e)
        {
            if (ThemePicker.SelectedIndex < 0) return;
            string selectedTheme = ThemePicker.SelectedIndex switch
            {
                1 => "Light",
                2 => "Dark",
                _ => "System"
            };
            Preferences.Set("AppTheme", selectedTheme);
            ApplyTheme(selectedTheme);
        }

        private void ApplyTheme(string theme)
        {
            if (Application.Current == null) return;
            Application.Current.UserAppTheme = theme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified // Systeemthema
            };
        }
    }
}