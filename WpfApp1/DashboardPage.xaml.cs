using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Helpers;
using WpfApp1.ViewModels;
using WpfApp1.Models;
using WpfApp1.Events;
using System.Linq;

namespace WpfApp1
{
    public partial class DashboardPage : Page
    {
        private readonly DatabaseHelper _dbHelper;
        private DashboardViewModel _dashboardViewModel;

        public DashboardPage(DatabaseHelper dbHelper)
        {
            InitializeComponent();
            _dbHelper = dbHelper;
            _dashboardViewModel = new DashboardViewModel(_dbHelper);
            DataContext = _dashboardViewModel;

            DatabaseChangedEvent.VehicleBrandDeleted += OnVehicleBrandDeleted;

            RefreshDataGrids();

            Textbox_SearchAppointments.TextChanged += Textbox_SearchAppointments_TextChanged;
            Textbox_SearchCustomers.TextChanged += Textbox_SearchCustomers_TextChanged;
        }

        private void Textbox_SearchAppointments_TextChanged(object sender, TextChangedEventArgs e)
        {
            _dashboardViewModel.AppointmentSearchText = Textbox_SearchAppointments.Text;
        }

        private void Textbox_SearchCustomers_TextChanged(object sender, TextChangedEventArgs e)
        {
            _dashboardViewModel.CustomerSearchText = Textbox_SearchCustomers.Text;
        }

        private void OnVehicleBrandDeleted(object sender, EventArgs e)
        {
            // Refresh the customer data grid
            _dashboardViewModel.CustomersVM.LoadCustomers();
        }

        private void RefreshDataGrids()
        {
            _dashboardViewModel.CustomersVM.LoadCustomers();
            _dashboardViewModel.AppointmentsVM.LoadAppointments();
        }

        private void button_AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CustomerEditPage(_dbHelper, null, RefreshDataGrids));
        }

        private void button_EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Customers.SelectedItem is Customer selectedCustomer)
            {
                var customerEditPage = new CustomerEditPage(_dbHelper, selectedCustomer, RefreshDataGrids);
                NavigationService.Navigate(customerEditPage);
            }
            else
            {
                MessageBox.Show("Please select a customer to edit.", "No Customer Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void button_DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Customers.SelectedItem is Customer selectedCustomer)
            {
                try
                {
                    // First, check if the customer has any appointments
                    int appointmentCount = _dbHelper.GetAppointmentCountForCustomer(selectedCustomer.Id);

                    if (appointmentCount > 0)
                    {
                        var result = MessageBox.Show(
                            $"{selectedCustomer.Name} has {appointmentCount} appointment(s). Deleting this customer will also delete all associated appointments. Are you sure you want to proceed?",
                            "Confirm Delete",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            _dbHelper.DeleteCustomer(selectedCustomer.Id, true);
                            RefreshDataGrids();
                            MessageBox.Show($"Customer and {appointmentCount} associated appointment(s) deleted successfully.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete {selectedCustomer.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            _dbHelper.DeleteCustomer(selectedCustomer.Id);
                            RefreshDataGrids();
                            MessageBox.Show("Customer deleted successfully.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.", "No Customer Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void button_EmptyDatabase_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to empty the database? This will delete all records.",
                                         "Confirm Empty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _dbHelper.PurgeDatabase();
                    RefreshDataGrids();
                    MessageBox.Show("Database emptied successfully.", "Database Emptied", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error emptying database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void button_ReloadDatagrids_Click(object sender, RoutedEventArgs e)
        {
            RefreshDataGrids();
            MessageBox.Show("Datagrids refreshed successfully.", "Datagrids Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void button_PopulateDatabase_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to populate the database with test data? This will only add data if the database is empty.",
                                         "Confirm Populate", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var seeder = new DatabaseSeeder(_dbHelper);
                    seeder.SeedDataIfNeeded();
                    RefreshDataGrids();

                    // Check if any new data was actually added
                    if (_dbHelper.GetCustomers().Any() || _dbHelper.GetTireBrands().Any())
                    {
                        MessageBox.Show("Database populated with test data.", "Populate Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Database already contained data. No new test data was added.", "No Action Taken", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error populating database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void button_Reports_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Reports(_dbHelper));
        }

        private void button_AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AppointmentsPage(_dbHelper));
        }

        private void button_EditAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Appointments.SelectedItem is Appointment selectedAppointment)
            {
                NavigationService.Navigate(new AppointmentsPage(_dbHelper, selectedAppointment, RefreshDataGrids));
            }
            else
            {
                MessageBox.Show("Please select an appointment to edit.", "No Appointment Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void button_DeleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Appointments.SelectedItem is Appointment selectedAppointment)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the appointment for {selectedAppointment.CustomerName} on {selectedAppointment.Date}?",
                                             "Confirm Delete",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbHelper.DeleteAppointment(selectedAppointment.Id);
                        RefreshDataGrids();
                        MessageBox.Show("Appointment deleted successfully.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting appointment: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an appointment to delete.", "No Appointment Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void dataGrid_Customers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid_Customers.SelectedItem is Customer selectedCustomer)
            {
                _dashboardViewModel.CustomersVM.CustomerOperations.SelectedCustomer = selectedCustomer;
            }
        }

        private void button_VehicleManagement_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new VehicleManagementPage(_dbHelper));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DatabaseChangedEvent.VehicleBrandDeleted -= OnVehicleBrandDeleted;
        }

        private void button_ExitApp_Click(object sender, RoutedEventArgs e)
        {
            //this button will cleanly exit the app 
            Application.Current.Shutdown();
        }

        private void button_LogOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.Logout();
            NavigationService.Navigate(new LoginPage(_dbHelper));
        }

        private void button_TireManagement_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TireManagementPage(_dbHelper));
        }
    }
}