﻿namespace bezorgapp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(PhotoGalleryPage), typeof(PhotoGalleryPage));
        }
    }
}