using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp1.Helpers;
using WpfApp1.Models;
using WpfApp1.Events;

namespace WpfApp1.ViewModels
{
    public class VehicleManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _dbHelper;

        // PROPERTIES //////////////////////////////////////////////////////////////////////////////////////

        public ObservableCollection<Vehicle> Vehicles { get; set; }
        public List<string> VehicleCategories { get; private set; }

        private Vehicle _selectedVehicle;
        public Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    OnPropertyChanged(nameof(SelectedVehicle));
                    UpdateFields();
                }
            }
        }

        private string _brand;
        public string Brand
        {
            get => _brand;
            set
            {
                if (_brand != value)
                {
                    _brand = value;
                    OnPropertyChanged(nameof(Brand));
                }
            }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilter();
                }
            }
        }

        // COMMANDS ////////////////////////////////////////////////////////////////////////////////////////

        public ICommand DeleteVehicleCommand { get; private set; }
        public ICommand ResetFieldsCommand { get; private set; }
        public ICommand SaveVehicleCommand { get; private set; }

        private ICollectionView _vehiclesView;

        // CONSTRUCTOR /////////////////////////////////////////////////////////////////////////////////////

        public VehicleManagementViewModel(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _dbHelper.EnsureVehicleCategories(); // Ensure categories are in the database
            Vehicles = new ObservableCollection<Vehicle>(_dbHelper.GetVehicles());
            VehicleCategories = _dbHelper.GetVehicleCategories();
            _vehiclesView = CollectionViewSource.GetDefaultView(Vehicles);

            InitializeCommands();
        }

        // INITIALIZATION //////////////////////////////////////////////////////////////////////////////////

        private void InitializeCommands()
        {
            DeleteVehicleCommand = new RelayCommand(_ => DeleteVehicle(), _ => CanDeleteVehicle());
            ResetFieldsCommand = new RelayCommand(_ => ResetFields());
            SaveVehicleCommand = new RelayCommand(_ => SaveVehicle(), _ => CanSaveVehicle());
        }

        // VEHICLE OPERATIONS //////////////////////////////////////////////////////////////////////////////

        private void ResetFields()
        {
            ClearFields();
        }

        private void SaveVehicle()
        {
            if (string.IsNullOrWhiteSpace(Brand) || string.IsNullOrWhiteSpace(SelectedCategory))
            {
                MessageBox.Show("Please enter both a brand and select a category.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedVehicle == null)
            {
                // Adding a new vehicle
                var result = MessageBox.Show($"Are you sure you want to add a new vehicle with brand '{Brand}' and category '{SelectedCategory}'?",
                    "Confirm Addition", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var newVehicle = new Vehicle { Brand = Brand, Category = SelectedCategory };
                    int newId = _dbHelper.AddOrUpdateVehicle(newVehicle);
                    newVehicle.Id = newId;
                    Vehicles.Add(newVehicle);
                    MessageBox.Show("New vehicle added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFields();
                }
            }
            else
            {
                // Editing an existing vehicle
                var result = MessageBox.Show($"Are you sure you want to update the vehicle '{SelectedVehicle.Brand}' to brand '{Brand}' and category '{SelectedCategory}'?",
                    "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedVehicle.Brand = Brand;
                    SelectedVehicle.Category = SelectedCategory;
                    _dbHelper.AddOrUpdateVehicle(SelectedVehicle);
                    RefreshData();
                    MessageBox.Show("Vehicle updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteVehicle()
        {
            if (SelectedVehicle != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the vehicle brand '{SelectedVehicle.Brand}'? This will remove the brand from all associated customers and appointments.",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbHelper.DeleteVehicleAndUpdateReferences(SelectedVehicle.Id);
                        Vehicles.Remove(SelectedVehicle);
                        ClearFields();
                        MessageBox.Show("Vehicle brand deleted successfully and removed from associated records.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        DatabaseChangedEvent.OnVehicleBrandDeleted();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting vehicle brand: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // HELPER METHODS //////////////////////////////////////////////////////////////////////////////////

        private bool CanDeleteVehicle() => SelectedVehicle != null;

        private bool CanSaveVehicle()
        {
            return !string.IsNullOrWhiteSpace(Brand) && !string.IsNullOrWhiteSpace(SelectedCategory);
        }

        private void ClearFields()
        {
            Brand = string.Empty;
            SelectedCategory = null;
            SelectedVehicle = null;
        }

        private void UpdateFields()
        {
            if (SelectedVehicle != null)
            {
                Brand = SelectedVehicle.Brand;
                SelectedCategory = SelectedVehicle.Category;
            }
        }

        private void RefreshData()
        {
            var updatedVehicles = _dbHelper.GetVehicles();
            Vehicles.Clear();
            foreach (var vehicle in updatedVehicles)
            {
                Vehicles.Add(vehicle);
            }
            VehicleCategories = _dbHelper.GetVehicleCategories();
            OnPropertyChanged(nameof(Vehicles));
            OnPropertyChanged(nameof(VehicleCategories));
        }

        private void ApplyFilter()
        {
            _vehiclesView.Filter = vehicle =>
            {
                var v = vehicle as Vehicle;
                if (v == null) return false;

                return string.IsNullOrWhiteSpace(SearchText) ||
                    v.Brand.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    v.Category.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            };
        }

        // PROPERTY CHANGED EVENT //////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}