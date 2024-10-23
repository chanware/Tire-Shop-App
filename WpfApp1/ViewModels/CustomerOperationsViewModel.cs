using System;
using System.ComponentModel;
using System.Windows.Input;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class CustomerOperationsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _dbHelper;
        private CustomersViewModel _customersViewModel;
        public event EventHandler CustomerUpdated;

        public CustomerOperationsViewModel(CustomersViewModel customersViewModel, DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _customersViewModel = customersViewModel;

            AddCustomerCommand = new RelayCommand(_ => AddCustomer());
            EditCustomerCommand = new RelayCommand(_ => EditCustomer(), _ => CanEditOrDeleteCustomer());
            DeleteCustomerCommand = new RelayCommand(_ => DeleteCustomer(), _ => CanEditOrDeleteCustomer());
        }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
                (EditCustomerCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCustomerCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddCustomerCommand { get; private set; }
        public ICommand EditCustomerCommand { get; private set; }
        public ICommand DeleteCustomerCommand { get; private set; }

        private void AddCustomer()
        {
            var newCustomer = new Customer { Name = "New Customer", Phone = "000-000-0000", Email = "new@example.com" };
            _dbHelper.AddOrUpdateCustomer(newCustomer);
            _customersViewModel.LoadCustomers();
            CustomerUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void EditCustomer()
        {
            if (SelectedCustomer != null)
            {
                SelectedCustomer.Name = "Updated " + SelectedCustomer.Name;
                _dbHelper.AddOrUpdateCustomer(SelectedCustomer);
                _customersViewModel.LoadCustomers();
                CustomerUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer != null)
            {
                _dbHelper.DeleteCustomer(SelectedCustomer.Id);
                _customersViewModel.LoadCustomers();
            }
        }

        private bool CanEditOrDeleteCustomer()
        {
            return SelectedCustomer != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}