using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp1.Helpers;
using WpfApp1.Models;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    public partial class VehicleManagementPage : Page
    {
        private readonly VehicleManagementViewModel _viewModel;

        public VehicleManagementPage(DatabaseHelper dbHelper)
        {
            InitializeComponent();
            _viewModel = new VehicleManagementViewModel(dbHelper);
            DataContext = _viewModel;
        }

        private void button_CancelVehicleInfo_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void UpdateVehicleIcon(string category)
        {
            string imagePath = $"/Images/{category.ToLower()}.png";
            VehicleIcon.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datagrid_VehicleManagement.SelectedItem is Vehicle selectedVehicle)
            {
                UpdateVehicleIcon(selectedVehicle.Category);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combobox_VehicleType.SelectedItem is string selectedCategory)
            {
                UpdateVehicleIcon(selectedCategory);
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
                button_CancelVehicleInfo_Click(this, new RoutedEventArgs());
            }
        }
    }
}