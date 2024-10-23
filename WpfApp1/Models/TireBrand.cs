using System.ComponentModel;

namespace WpfApp1.Models
{
    public class TireBrand : INotifyPropertyChanged
    {
        public int Id { get; set; }

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

        private string _tireCategory;
        public string TireCategory
        {
            get => _tireCategory;
            set
            {
                if (_tireCategory != value)
                {
                    _tireCategory = value;
                    OnPropertyChanged(nameof(TireCategory));
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}