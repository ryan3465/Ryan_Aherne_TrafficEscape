namespace TrafficEscape2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            ApplySavedTheme();
           // MainPage = new NavigationPage(new MainPage());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void ApplySavedTheme()
        {
            bool isDarkMode = Preferences.Get("IsDarkMode", false);
            Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }
}