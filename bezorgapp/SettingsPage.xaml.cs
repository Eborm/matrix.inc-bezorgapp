namespace bezorgapp
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            bool isDarkMode = Preferences.Get("DarkModeEnabled", false);
            ThemeSwitch.IsToggled = isDarkMode;
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
            }
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