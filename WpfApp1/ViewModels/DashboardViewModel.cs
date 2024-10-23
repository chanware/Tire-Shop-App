using System;
using System.ComponentModel;
using WpfApp1.Events;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public CustomersViewModel CustomersVM { get; set; }
        public AppointmentsViewModel AppointmentsVM { get; set; }
        private readonly DatabaseHelper _dbHelper;

        private string _appointmentSearchText;
        public string AppointmentSearchText
        {
            get => _appointmentSearchText;
            set
            {
                if (_appointmentSearchText != value)
                {
                    _appointmentSearchText = value;
                    OnPropertyChanged(nameof(AppointmentSearchText));
                    ApplyAppointmentFilter();
                }
            }
        }

        private string _customerSearchText;
        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                if (_customerSearchText != value)
                {
                    _customerSearchText = value;
                    OnPropertyChanged(nameof(CustomerSearchText));
                    ApplyCustomerFilter();
                }
            }
        }

        public DashboardViewModel(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            CustomersVM = new CustomersViewModel(dbHelper);
            AppointmentsVM = new AppointmentsViewModel(dbHelper);
            CustomersVM.CustomerOperations.CustomerUpdated += OnCustomerUpdated;
            TireBrandDeletedEvent.TireBrandDeleted += OnTireBrandDeleted;
            SetupFilters();
        }

        private void OnCustomerUpdated(object sender, EventArgs e)
        {
            CustomersVM.LoadCustomers();
            AppointmentsVM.LoadAppointments();
        }

        private void SetupFilters()
        {
            CustomersVM.CustomersView.Filter = CustomerFilter;
            AppointmentsVM.AppointmentsView.Filter = AppointmentFilter;
        }


        private bool CustomerFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(CustomerSearchText))
                return true;

            if (item is Customer customer)
            {
                return ContainsIgnoreCase(customer.Name, CustomerSearchText) ||
                       ContainsIgnoreCase(customer.Email, CustomerSearchText) ||
                       ContainsIgnoreCase(customer.Phone, CustomerSearchText) ||
                       ContainsIgnoreCase(customer.Address, CustomerSearchText) ||
                       ContainsIgnoreCase(customer.City, CustomerSearchText) ||
                       ContainsIgnoreCase(customer.Country, CustomerSearchText) ||
                       ContainsIgnoreCase(customer.VehicleBrand, CustomerSearchText) ||
                       ContainsIgnoreCase(customer.VehicleCategory, CustomerSearchText);
            }

            return false;
        }

        private bool AppointmentFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(AppointmentSearchText))
                return true;

            if (item is Appointment appointment)
            {
                return ContainsIgnoreCase(appointment.Date, AppointmentSearchText) ||
                       ContainsIgnoreCase(appointment.Description, AppointmentSearchText) ||
                       ContainsIgnoreCase(appointment.SelectedTireBrand, AppointmentSearchText) ||
                       ContainsIgnoreCase(appointment.CustomerName, AppointmentSearchText) ||
                       ContainsIgnoreCase(appointment.CustomerId.ToString(), AppointmentSearchText);
            }

            return false;
        }

        private bool ContainsIgnoreCase(string source, string toCheck)
        {
            return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ApplyCustomerFilter()
        {
            CustomersVM.CustomersView.Refresh();
        }

        private void ApplyAppointmentFilter()
        {
            AppointmentsVM.AppointmentsView.Refresh();
        }

        public void ResetSearches()
        {
            CustomerSearchText = string.Empty;
            AppointmentSearchText = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnTireBrandDeleted(object sender, EventArgs e)
        {
            AppointmentsVM.LoadAppointments();
        }


        public void Cleanup()
        {
            CustomersVM.CustomerOperations.CustomerUpdated -= OnCustomerUpdated;
            TireBrandDeletedEvent.TireBrandDeleted -= OnTireBrandDeleted;
        }

    }
}