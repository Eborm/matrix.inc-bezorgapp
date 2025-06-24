// AppShell.xaml.cs
// Deze klasse definieert de navigatiestructuur van de applicatie.

namespace bezorgapp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Registreer routes voor navigatie naar pagina's binnen de app
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(PhotoGalleryPage), typeof(PhotoGalleryPage));
        }
    }
}