using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace WpfApp1.ViewModels
{
    public class DateTimeViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private string _currentDateTime;

        public string CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    OnPropertyChanged(nameof(CurrentDateTime));
                }
            }
        }

        public DateTimeViewModel()
        {
            // Initialize the date and time
            UpdateDateTime();

            // Set up a timer to update the time every minute
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // Update every minute
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            // Update the CurrentDateTime property with date and time (no seconds)
            CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy - hh:mm tt");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}