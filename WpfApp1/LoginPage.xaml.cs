using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class LoginPage : Page
    {
        private readonly DatabaseHelper _dbHelper;

        public LoginPage(DatabaseHelper dbHelper)
        {
            InitializeComponent();
            _dbHelper = dbHelper;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = textBox_Username.Text;
            string password = passwordbox_UserPassword.Password;

            if (_dbHelper.VerifyUser(username, password))
            {
                SessionManager.Login(new User { Username = username });
                NavigateToDashboard();
            }
            else
            {
                MessageBox.Show("Invalid username or password", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToDashboard()
        {
            var dashboardPage = new DashboardPage(_dbHelper);
            NavigationService.Navigate(dashboardPage);
        }

        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_Username.Text == "Username")
            {
                textBox_Username.Text = "";
            }
        }

        private void Username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Username.Text))
            {
                textBox_Username.Text = "Username";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Allow pressing Enter key to login
        private void passwordbox_UserPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(this, new RoutedEventArgs());
            }
        }

    }
}