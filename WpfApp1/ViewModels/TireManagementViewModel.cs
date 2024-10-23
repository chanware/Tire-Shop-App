using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using WpfApp1.Helpers;
using WpfApp1.Models;
using System.Collections.Generic;
using System.Windows;
using WpfApp1.Events;

namespace WpfApp1.ViewModels
{
    public class TireManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _dbHelper;

        public ObservableCollection<TireBrand> TireBrands { get; set; }
        public List<string> VehicleCategories { get; set; }

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

        private string _selectedFilter;
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter != value)
                {
                    _selectedFilter = value;
                    OnPropertyChanged(nameof(SelectedFilter));
                    ApplyFilter();
                }
            }
        }

        private string _restockAlert;
        public string RestockAlert
        {
            get => _restockAlert;
            set
            {
                if (_restockAlert != value)
                {
                    _restockAlert = value;
                    OnPropertyChanged(nameof(RestockAlert));
                }
            }
        }

        public List<string> FilterOptions { get; } = new List<string>
        {
            "All", "Low stock only", "Truck", "SUV", "Sedan", "Electric"
        };

        public ICommand SearchCommand { get; private set; }
        public ICommand ResetSearchCommand { get; private set; }
        public ICommand ResetFieldsCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        private ICollectionView _tireBrandsView;

        private TireBrand _selectedTireBrand;
        public TireBrand SelectedTireBrand
        {
            get => _selectedTireBrand;
            set
            {
                if (_selectedTireBrand != value)
                {
                    _selectedTireBrand = value;
                    OnPropertyChanged(nameof(SelectedTireBrand));
                    UpdateFields();
                }
            }
        }

        private string _brandName;
        public string BrandName
        {
            get => _brandName;
            set
            {
                if (_brandName != value)
                {
                    _brandName = value;
                    OnPropertyChanged(nameof(BrandName));
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

        private int _stockQuantity;
        public int StockQuantity
        {
            get => _stockQuantity;
            set
            {
                if (_stockQuantity != value)
                {
                    _stockQuantity = value;
                    OnPropertyChanged(nameof(StockQuantity));
                }
            }
        }

        public TireManagementViewModel(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            TireBrands = new ObservableCollection<TireBrand>(_dbHelper.GetTireBrands());
            _tireBrandsView = CollectionViewSource.GetDefaultView(TireBrands);
            VehicleCategories = _dbHelper.GetVehicleCategories();

            SearchCommand = new RelayCommand(_ => ApplyFilter());
            ResetSearchCommand = new RelayCommand(_ => ResetSearch());
            ResetFieldsCommand = new RelayCommand(_ => ResetFields());
            SaveCommand = new RelayCommand(_ => SaveTireBrand(), _ => CanSaveTireBrand());
            DeleteCommand = new RelayCommand(_ => DeleteTireBrand(), _ => CanDeleteTireBrand());

            SelectedFilter = "All";
            ApplyFilter();
            UpdateRestockAlert();
        }

        private void ApplyFilter()
        {
            _tireBrandsView.Filter = tire =>
            {
                var tireBrand = tire as TireBrand;
                if (tireBrand == null) return false;

                bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                    tireBrand.BrandName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    tireBrand.TireCategory.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    tireBrand.StockQuantity.ToString().IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;

                bool matchesFilter = SelectedFilter == "All" ||
                    (SelectedFilter == "Low stock only" && tireBrand.StockQuantity <= 3) ||
                    SelectedFilter == tireBrand.TireCategory;

                return matchesSearch && matchesFilter;
            };

            UpdateRestockAlert();
        }

        public void ResetSearch()
        {
            SearchText = string.Empty;
            SelectedFilter = "All";
            ApplyFilter();
        }

        private void UpdateRestockAlert()
        {
            var lowStockBrands = TireBrands.Where(t => t.StockQuantity <= 3 &&
                (_tireBrandsView.Filter == null || _tireBrandsView.Filter(t))).ToList();

            if (lowStockBrands.Any())
            {
                RestockAlert = string.Join("\n", lowStockBrands.Select(t => $"{t.BrandName} ({t.StockQuantity} sets)"));
            }
            else
            {
                RestockAlert = "No restock alerts at this time.";
            }
        }

        private void ResetFields()
        {
            SelectedTireBrand = null;  // This will clear the DataGrid selection
            BrandName = string.Empty;
            SelectedCategory = null;
            StockQuantity = 0;
            OnPropertyChanged(nameof(SelectedTireBrand));
            OnPropertyChanged(nameof(BrandName));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(StockQuantity));
        }

        private void UpdateFields()
        {
            if (SelectedTireBrand != null)
            {
                BrandName = SelectedTireBrand.BrandName;
                SelectedCategory = SelectedTireBrand.TireCategory;
                StockQuantity = SelectedTireBrand.StockQuantity;
            }
            else
            {
                BrandName = string.Empty;
                SelectedCategory = null;
                StockQuantity = 0;
            }
            OnPropertyChanged(nameof(BrandName));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(StockQuantity));
        }

        private bool CanSaveTireBrand()
        {
            return !string.IsNullOrWhiteSpace(BrandName) && !string.IsNullOrWhiteSpace(SelectedCategory);
        }

        private void SaveTireBrand()
        {
            try
            {
                // Check if a tire brand for this category already exists
                var existingBrand = TireBrands.FirstOrDefault(t => t.TireCategory == SelectedCategory && t.Id != (SelectedTireBrand?.Id ?? 0));
                if (existingBrand != null)
                {
                    MessageBox.Show($"A tire brand '{existingBrand.BrandName}' already exists for the {SelectedCategory} category. Only one brand per category is allowed.", "Duplicate Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string actionType = SelectedTireBrand == null ? "add" : "update";
                string confirmMessage = SelectedTireBrand == null
                    ? $"Are you sure you want to add a new tire brand '{BrandName}' for {SelectedCategory}?"
                    : $"Are you sure you want to update the tire brand '{SelectedTireBrand.BrandName}' for {SelectedCategory}?";

                var result = MessageBox.Show(confirmMessage, $"Confirm {actionType}", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (SelectedTireBrand == null)
                    {
                        // Adding a new tire brand
                        var newTireBrand = new TireBrand
                        {
                            BrandName = BrandName,
                            TireCategory = SelectedCategory,
                            StockQuantity = StockQuantity
                        };
                        _dbHelper.AddOrUpdateTireBrand(newTireBrand);
                        TireBrands.Add(newTireBrand);
                    }
                    else
                    {
                        // Updating existing tire brand
                        SelectedTireBrand.BrandName = BrandName;
                        SelectedTireBrand.TireCategory = SelectedCategory;
                        SelectedTireBrand.StockQuantity = StockQuantity;
                        _dbHelper.AddOrUpdateTireBrand(SelectedTireBrand);
                    }

                    MessageBox.Show($"Tire brand {actionType}d successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetFields();
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tire brand: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanDeleteTireBrand()
        {
            return SelectedTireBrand != null;
        }

        private void DeleteTireBrand()
        {
            if (SelectedTireBrand != null)
            {
                try
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete the tire brand '{SelectedTireBrand.BrandName}'?\n\n" +
                        "This will remove the tire brand from all associated appointments.",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        _dbHelper.DeleteTireBrandAndUpdateReferences(SelectedTireBrand.Id);
                        TireBrands.Remove(SelectedTireBrand);
                        MessageBox.Show(
                            "Tire brand deleted successfully and removed from associated appointments.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                        ResetFields();
                        ApplyFilter();
                        TireBrandDeletedEvent.OnTireBrandDeleted(); // Raise the event
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting tire brand: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}