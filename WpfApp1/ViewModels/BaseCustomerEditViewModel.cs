using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{

    // The purpose of this class is to provide a base class
    // for the CustomerEditViewModel and VehicleEditViewModel classes.
    public abstract class BaseCustomerEditViewModel : INotifyPropertyChanged
    {
        protected readonly DatabaseHelper _dbHelper;

        public BaseCustomerEditViewModel(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            VehicleCategories = _dbHelper.GetVehicleCategories();
            LoadVehicleBrands();
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        private string _address2;
        public string Address2
        {
            get => _address2;
            set { _address2 = value; OnPropertyChanged(nameof(Address2)); }
        }

        private string _city;
        public string City
        {
            get => _city;
            set { _city = value; OnPropertyChanged(nameof(City)); }
        }

        private string _country;
        public string Country
        {
            get => _country;
            set { _country = value; OnPropertyChanged(nameof(Country)); }
        }

        private int? _vehicleBrandId;
        public int? VehicleBrandId
        {
            get => _vehicleBrandId;
            set { _vehicleBrandId = value; OnPropertyChanged(nameof(VehicleBrandId)); }
        }

        private string _vehicleIconPath;
        public string VehicleIconPath
        {
            get => _vehicleIconPath;
            set
            {
                _vehicleIconPath = value;
                OnPropertyChanged(nameof(VehicleIconPath));
            }
        }

        protected void UpdateVehicleIcon()
        {
            var vehicle = _dbHelper.GetVehicles().FirstOrDefault(v => v.Brand == SelectedVehicleBrand);
            if (vehicle != null)
            {
                VehicleIconPath = $"/Images/{vehicle.Category.ToLower()}.png";
            }
            else
            {
                VehicleIconPath = null;
            }
        }

        public void OnVehicleBrandChanged()
        {
            UpdateVehicleIcon();
        }




        private ObservableCollection<string> _vehicleBrands;
        public ObservableCollection<string> VehicleBrands
        {
            get => _vehicleBrands;
            set { _vehicleBrands = value; OnPropertyChanged(nameof(VehicleBrands)); }
        }

        private string _selectedVehicleBrand;
        public string SelectedVehicleBrand
        {
            get => _selectedVehicleBrand;
            set
            {
                _selectedVehicleBrand = value;
                OnPropertyChanged(nameof(SelectedVehicleBrand));
                UpdateVehicleCategory();
            }
        }

        private bool _isVehicleCategoryEnabled;
        public bool IsVehicleCategoryEnabled
        {
            get => _isVehicleCategoryEnabled;
            set { _isVehicleCategoryEnabled = value; OnPropertyChanged(nameof(IsVehicleCategoryEnabled)); }
        }

        private List<string> _vehicleCategories;
        public List<string> VehicleCategories
        {
            get => _vehicleCategories;
            set { _vehicleCategories = value; OnPropertyChanged(nameof(VehicleCategories)); }
        }

        private string _selectedVehicleCategory;
        public string SelectedVehicleCategory
        {
            get => _selectedVehicleCategory;
            set { _selectedVehicleCategory = value; OnPropertyChanged(nameof(SelectedVehicleCategory)); }
        }

        private void LoadVehicleBrands()
        {
            var brands = _dbHelper.GetVehicleBrands();
            VehicleBrands = new ObservableCollection<string>(brands);
        }

        private void UpdateVehicleCategory()
        {
            var vehicle = _dbHelper.GetVehicles().FirstOrDefault(v => v.Brand == SelectedVehicleBrand);
            if (vehicle != null)
            {
                SelectedVehicleCategory = vehicle.Category;
                IsVehicleCategoryEnabled = false;
            }
            else
            {
                SelectedVehicleCategory = null;
                IsVehicleCategoryEnabled = true;
            }
        }

        public abstract void Save();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}