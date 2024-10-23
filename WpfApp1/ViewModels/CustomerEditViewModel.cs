using System;
using System.Linq;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class CustomerEditViewModel : BaseCustomerEditViewModel
    {
        private int _customerId;
        private string _originalVehicleBrand;

        public CustomerEditViewModel(DatabaseHelper dbHelper, Customer customer) : base(dbHelper)
        {
            _customerId = customer.Id;
            Name = customer.Name;
            Phone = customer.Phone;
            Email = customer.Email;
            Address = customer.Address;
            Address2 = customer.Address2;
            City = customer.City;
            Country = customer.Country;
            VehicleBrandId = customer.VehicleBrandId;
            SelectedVehicleBrand = customer.VehicleBrand;
            _originalVehicleBrand = customer.VehicleBrand;
            SelectedVehicleCategory = customer.VehicleCategory;

            VehicleCategories = _dbHelper.GetVehicleCategories();
        }

        public string OriginalVehicleBrand => _originalVehicleBrand;


        public event EventHandler<CustomerUpdatedEventArgs> CustomerUpdated;

        public override void Save()
        {
            var customer = new Customer
            {
                Id = _customerId,
                Name = Name,
                Phone = Phone,
                Email = Email,
                Address = Address,
                Address2 = Address2,
                City = City,
                Country = Country,
                VehicleBrand = SelectedVehicleBrand,
                VehicleCategory = SelectedVehicleCategory
            };

            var vehicle = _dbHelper.GetVehicles().FirstOrDefault(v => v.Brand == SelectedVehicleBrand);
            if (vehicle != null)
            {
                customer.VehicleBrandId = vehicle.Id;
            }

            _dbHelper.AddOrUpdateCustomer(customer);
            CustomerUpdated?.Invoke(this, new CustomerUpdatedEventArgs(customer));
        }
    }

    public class CustomerUpdatedEventArgs : EventArgs
    {
        public Customer UpdatedCustomer { get; }

        public CustomerUpdatedEventArgs(Customer updatedCustomer)
        {
            UpdatedCustomer = updatedCustomer;
        }
    }
}