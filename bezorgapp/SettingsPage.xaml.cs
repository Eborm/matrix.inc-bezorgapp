namespace bezorgapp
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            // Only set the toggle state - theme is already applied at app startup
            bool isDarkMode = Preferences.Get("DarkModeEnabled", false);
            ThemeSwitch.IsToggled = isDarkMode;
        }
        
        private void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
                Preferences.Set("DarkModeEnabled", e.Value);
            }
        }
    }
}