using WpfApp1.Helpers;
using WpfApp1.Models;
using System.Linq;

namespace WpfApp1.ViewModels
{
    public class CustomerAddViewModel : BaseCustomerEditViewModel
    {
        public CustomerAddViewModel(DatabaseHelper dbHelper) : base(dbHelper)
        {
        }

        public override void Save()
        {
            var customer = new Customer
            {
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
        }
    }
}