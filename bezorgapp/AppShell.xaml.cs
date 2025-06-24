namespace bezorgapp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
            Routing.RegisterRoute(nameof(PhotoGalleryPage), typeof(PhotoGalleryPage));
            Routing.RegisterRoute(nameof(PhotoDetailPage), typeof(PhotoDetailPage));
            Routing.RegisterRoute(nameof(CreatePicture), typeof(CreatePicture));
            Routing.RegisterRoute(nameof(BarcodeScannerPage), typeof(BarcodeScannerPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}