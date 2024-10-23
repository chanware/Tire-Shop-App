using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Helpers;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    public partial class TireManagementPage : Page
    {
        private readonly TireManagementViewModel _viewModel;

        public TireManagementPage(DatabaseHelper dbHelper)
        {
            InitializeComponent();
            _viewModel = new TireManagementViewModel(dbHelper);
            DataContext = _viewModel;
        }

        private void button_CancelTireManagement_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;
            Focus();
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                button_CancelTireManagement_Click(this, new RoutedEventArgs());
            }
        }
    }
}