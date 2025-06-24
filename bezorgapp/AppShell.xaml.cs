// AppShell.xaml.csAdd commentMore actions
// Deze klasse definieert de navigatiestructuur van de applicatie.

namespace bezorgapp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Registreer routes voor navigatie naar pagina's binnen de app
            Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
            Routing.RegisterRoute(nameof(PhotoGalleryPage), typeof(PhotoGalleryPage));
            Routing.RegisterRoute(nameof(PhotoDetailPage), typeof(PhotoDetailPage));
            Routing.RegisterRoute(nameof(CreatePicture), typeof(CreatePicture));
            Routing.RegisterRoute(nameof(BarcodeScannerPage), typeof(BarcodeScannerPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}