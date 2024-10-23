using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class CustomersViewModel : INotifyPropertyChanged
    {
        public ICollectionView CustomersView { get; private set; }
        private ObservableCollection<Customer> _customers;
        public CustomerOperationsViewModel CustomerOperations { get; private set; }

        private readonly DatabaseHelper _dbHelper;

        public CustomersViewModel(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _customers = new ObservableCollection<Customer>();
            CustomersView = CollectionViewSource.GetDefaultView(_customers);
            CustomerOperations = new CustomerOperationsViewModel(this, dbHelper);
            LoadCustomers();
        }

        public void LoadCustomers()
        {
            _customers.Clear();
            var customers = _dbHelper.GetCustomers();
            foreach (var customer in customers)
            {
                _customers.Add(customer);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}