using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.ViewModels;
using WpfApp1.Models;
using WpfApp1.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace WpfApp1
{
    public partial class CustomerEditPage : Page
    {
        private BaseCustomerEditViewModel _viewModel;
        private readonly DatabaseHelper _dbHelper;
        private readonly Action _refreshCallback;

        public CustomerEditPage(DatabaseHelper dbHelper, Customer customer = null, Action refreshCallback = null)
        {
            InitializeComponent();
            _dbHelper = dbHelper;
            _refreshCallback = refreshCallback;

            if (customer == null)
            {
                _viewModel = new CustomerAddViewModel(_dbHelper);
                this.Title = "Add Customer";
            }
            else
            {
                _viewModel = new CustomerEditViewModel(_dbHelper, customer);
                this.Title = "Edit Customer";
            }

            DataContext = _viewModel;

            InitializeCountries();
            SetupEventHandlers();
            PopulateFields();
        }

        private void PopulateFields()
        {
            textbox_CustomerName.Text = _viewModel.Name;
            textbox_CustomerEmail.Text = _viewModel.Email;
            textbox_CustomerPhone.Text = _viewModel.Phone;
            textbox_CustomerAddress1.Text = _viewModel.Address;
            textbox_CustomerAddress2.Text = _viewModel.Address2;
            textbox_CustomerCity.Text = _viewModel.City;
            combobox_CustomerCountry.SelectedItem = _viewModel.Country;
            combobox_CustomerVehicleBrand.SelectedItem = _viewModel.SelectedVehicleBrand;
        }

        private void ComboBox_CustomerVehicleBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combobox_CustomerVehicleBrand.SelectedItem is string selectedBrand)
            {
                _viewModel.SelectedVehicleBrand = selectedBrand;
            }
        }

        private void UpdateViewModelFromUI()
        {
            _viewModel.Name = textbox_CustomerName.Text;
            _viewModel.Email = textbox_CustomerEmail.Text;
            _viewModel.Phone = textbox_CustomerPhone.Text;
            _viewModel.Address = textbox_CustomerAddress1.Text;
            _viewModel.Address2 = textbox_CustomerAddress2.Text;
            _viewModel.City = textbox_CustomerCity.Text;
            _viewModel.Country = combobox_CustomerCountry.SelectedItem as string;

            var selectedBrand = combobox_CustomerVehicleBrand.SelectedItem as string;

            _viewModel.SelectedVehicleBrand = selectedBrand;
        }

        private void button_SaveCustomerEdits_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                UpdateViewModelFromUI();
                _viewModel.Save();
                _refreshCallback?.Invoke();
                NavigateBack();
            }
        }

        private void InitializeCountries()
        {
            combobox_CustomerCountry.ItemsSource = new[] { "USA", "Canada", "Mexico" };
        }

        private void SetupEventHandlers()
        {
            combobox_CustomerVehicleBrand.SelectionChanged += (sender, e) =>
            {
                if (combobox_CustomerVehicleBrand.SelectedItem is string selectedBrand)
                {
                    _viewModel.SelectedVehicleBrand = selectedBrand;
                }
            };
        }

        private bool HasUnsavedChanges()
        {
            if (_viewModel is CustomerAddViewModel)
            {
                return !string.IsNullOrWhiteSpace(textbox_CustomerName.Text) ||
                       !string.IsNullOrWhiteSpace(textbox_CustomerEmail.Text) ||
                       !string.IsNullOrWhiteSpace(textbox_CustomerPhone.Text) ||
                       !string.IsNullOrWhiteSpace(textbox_CustomerAddress1.Text) ||
                       !string.IsNullOrWhiteSpace(textbox_CustomerCity.Text) ||
                       combobox_CustomerCountry.SelectedItem != null ||
                       combobox_CustomerVehicleBrand.SelectedItem != null;
            }
            else if (_viewModel is CustomerEditViewModel editViewModel)
            {
                var selectedVehicleBrand = combobox_CustomerVehicleBrand.SelectedItem as string;
                Debug.WriteLine($"Selected Vehicle Brand: {selectedVehicleBrand}");
                Debug.WriteLine($"Original Vehicle Brand: {editViewModel.OriginalVehicleBrand}");

                bool hasChanges = textbox_CustomerName.Text != editViewModel.Name ||
                       textbox_CustomerEmail.Text != editViewModel.Email ||
                       textbox_CustomerPhone.Text != editViewModel.Phone ||
                       textbox_CustomerAddress1.Text != editViewModel.Address ||
                       textbox_CustomerAddress2.Text != editViewModel.Address2 ||
                       textbox_CustomerCity.Text != editViewModel.City ||
                       combobox_CustomerCountry.SelectedItem as string != editViewModel.Country ||
                       selectedVehicleBrand != editViewModel.OriginalVehicleBrand;

                Debug.WriteLine($"Has Unsaved Changes: {hasChanges}");
                return hasChanges;
            }
            return false;
        }
        private void button_CancelCustomerEdits_Click(object sender, RoutedEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("You have unsaved changes. Are you sure you want to go back without saving?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            NavigateBack();
        }


        // INPUT VALIDATION ////////////////////////////////////////////////////////////////
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(textbox_CustomerName.Text) ||
                string.IsNullOrWhiteSpace(textbox_CustomerEmail.Text) ||
                string.IsNullOrWhiteSpace(textbox_CustomerPhone.Text) ||
                string.IsNullOrWhiteSpace(textbox_CustomerAddress1.Text) ||
                string.IsNullOrWhiteSpace(textbox_CustomerCity.Text) ||
                combobox_CustomerCountry.SelectedItem == null ||
                combobox_CustomerVehicleBrand.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsValidEmail(textbox_CustomerEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsValidPhone(textbox_CustomerPhone.Text))
            {
                MessageBox.Show("Please enter a valid phone number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[\d-]+$");
        }

        private void textbox_CustomerPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "-";
        }

        private void NavigateBack()
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
                button_CancelCustomerEdits_Click(this, new RoutedEventArgs());
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combobox_CustomerVehicleBrand.SelectedItem is string selectedBrand)
            {
                Debug.WriteLine($"Vehicle Brand Selection Changed: {selectedBrand}");
                _viewModel.SelectedVehicleBrand = selectedBrand;
                _viewModel.OnVehicleBrandChanged();
                UpdateVehicleIcon();
            }
        }

        private void UpdateVehicleIcon()
        {
            if (!string.IsNullOrEmpty(_viewModel.VehicleIconPath))
            {
                VehicleIcon.Source = new BitmapImage(new Uri(_viewModel.VehicleIconPath, UriKind.Relative));
            }
            else
            {
                VehicleIcon.Source = null;
            }
        }



    }
}