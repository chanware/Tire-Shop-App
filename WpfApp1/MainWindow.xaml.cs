using System.Windows;
using WpfApp1.ViewModels;
using WpfApp1.Helpers;
using static WpfApp1.Helpers.DatabaseHelper;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseHelper _dbHelper;

        public MainWindow()
        {
            InitializeComponent();

            // Set DataContext to DateTimeViewModel for the current date/time binding
            DataContext = new DateTimeViewModel();

            // Create an instance of DatabaseHelper
            _dbHelper = new DatabaseHelper();

            // Seed the database if needed
            var seeder = new DatabaseSeeder(_dbHelper);
            seeder.SeedDataIfNeeded();

            // Initially navigate to the LoginPage
            frame_MainWindow.NavigationService.Navigate(new LoginPage(_dbHelper));

            // Subscribe to the SessionManager's LoginStateChanged event
            SessionManager.LoginStateChanged += OnLoginStateChanged;

            // Initialize the user display
            UpdateUserDisplay();
        }

        private void OnLoginStateChanged(object sender, System.EventArgs e)
        {
            UpdateUserDisplay();
        }

        private void UpdateUserDisplay()
        {
            if (SessionManager.IsLoggedIn())
            {
                textbox_CurrentUser.Text = SessionManager.CurrentUser.Username;
            }
            else
            {
                textbox_CurrentUser.Text = "Not logged in";
            }
        }

        // Function to navigate to the DashboardPage after successful login
        public void NavigateToDashboard()
        {
            frame_MainWindow.NavigationService.Navigate(new DashboardPage(_dbHelper));
        }
    }
}