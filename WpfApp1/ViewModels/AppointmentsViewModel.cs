using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class AppointmentsViewModel : INotifyPropertyChanged
    {
        public ICollectionView AppointmentsView { get; private set; }
        private ObservableCollection<Appointment> _appointments;

        private readonly DatabaseHelper _dbHelper;

        public AppointmentsViewModel(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _appointments = new ObservableCollection<Appointment>();
            AppointmentsView = CollectionViewSource.GetDefaultView(_appointments);
            LoadAppointments();
        }

        public void LoadAppointments()
        {
            _appointments.Clear();
            var appointments = _dbHelper.GetAppointments();
            foreach (var appointment in appointments)
            {
                var customer = _dbHelper.GetCustomers().FirstOrDefault(c => c.Id == appointment.CustomerId);
                if (customer != null)
                {
                    appointment.SelectedTireBrand = _dbHelper.GetRecommendedTireBrand(customer.VehicleCategory);
                }
                _appointments.Add(appointment);
            }
            AppointmentsView.Refresh();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}