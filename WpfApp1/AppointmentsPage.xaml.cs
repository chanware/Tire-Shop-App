using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Helpers;
using WpfApp1.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace WpfApp1
{
    public partial class AppointmentsPage : Page
    {
        private readonly DatabaseHelper _dbHelper;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<string> _timeSlots;
        private Appointment _existingAppointment;
        private Action _refreshCallback;

        public AppointmentsPage(DatabaseHelper dbHelper, Appointment existingAppointment = null, Action refreshCallback = null)
        {
            InitializeComponent();
            _dbHelper = dbHelper;
            _existingAppointment = existingAppointment;
            _refreshCallback = refreshCallback;

            LoadCustomers();
            InitializeTimeSlots();
            SetupEventHandlers();
            InitializeReadOnlyFields();

            if (_existingAppointment != null)
            {
                PopulateExistingAppointmentData();
            }
        }

        private void InitializeReadOnlyFields()
        {
            textbox_Employee.IsReadOnly = true;
            textbox_CustomerVehicle.IsReadOnly = true;
            textbox_VehicleType.IsReadOnly = true;
            textbox_RecommendedTires.IsReadOnly = true;
            textbox_TiresInStock.IsReadOnly = true;
        }

        private void PopulateExistingAppointmentData()
        {
            LogDebugInfo($"Populating existing appointment data. CustomerId: {_existingAppointment.CustomerId}");
            var customer = _customers.FirstOrDefault(c => c.Id == _existingAppointment.CustomerId);
            if (customer != null)
            {
                combobox_Customer.SelectedItem = customer;
            }

            datepicker_AppointmentDate.SelectedDate = DateTime.Parse(_existingAppointment.Date);

            DateTime appointmentTime = DateTime.Parse(_existingAppointment.Date);
            string timeSlot = $"{appointmentTime:hh:mm tt}";
            combobox_AppointmentTime.SelectedItem = _timeSlots.FirstOrDefault(ts => ts.StartsWith(timeSlot));

            textbox_AppointmentDescription.Text = _existingAppointment.Description;
        }

        private void LoadCustomers()
        {
            LogDebugInfo("Loading customers");
            _customers = new ObservableCollection<Customer>(_dbHelper.GetCustomers());
            combobox_Customer.ItemsSource = _customers;
            combobox_Customer.DisplayMemberPath = "Name";
            LogDebugInfo($"Loaded {_customers.Count} customers");
        }

        private void InitializeTimeSlots()
        {
            _timeSlots = new ObservableCollection<string>();
            for (int hour = 8; hour < 18; hour++)
            {
                string period = hour < 12 ? "AM" : "PM";
                int displayHour = hour > 12 ? hour - 12 : hour;
                if (hour == 12) displayHour = 12; // Handle noon correctly

                _timeSlots.Add($"{displayHour:D2}:00 {period}");
                _timeSlots.Add($"{displayHour:D2}:30 {period}");
            }
            combobox_AppointmentTime.ItemsSource = _timeSlots;
        }

        private void SetupEventHandlers()
        {
            combobox_Customer.SelectionChanged += ComboBox_Customer_SelectionChanged;
            button_SaveAppointment.Click += Button_SaveAppointment_Click;
            Button_CancelEditAppt.Click += Button_CancelEditAppt_Click;

            // Set the employee name (for now, we'll use a hardcoded value)
            textbox_Employee.Text = "CarloSr";
        }

        private void ComboBox_Customer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogDebugInfo("Customer selection changed");
            if (combobox_Customer.SelectedItem is Customer selectedCustomer)
            {
                textbox_CustomerVehicle.Text = selectedCustomer.VehicleBrand;
                textbox_VehicleType.Text = selectedCustomer.VehicleCategory;
                UpdateRecommendedTires(selectedCustomer.VehicleCategory);
            }
            else
            {
                ClearAppointmentFields();
            }
        }


        private void UpdateRecommendedTires(string vehicleCategory)
        {
            string recommendedTire = _dbHelper.GetRecommendedTireBrand(vehicleCategory);
            textbox_RecommendedTires.Text = recommendedTire;

            if (!string.IsNullOrEmpty(recommendedTire))
            {
                var tireBrand = _dbHelper.GetTireBrands().FirstOrDefault(tb => tb.BrandName == recommendedTire);
                textbox_TiresInStock.Text = tireBrand?.StockQuantity.ToString() ?? "0";
            }
            else
            {
                textbox_TiresInStock.Text = "0";
            }
        }



        private void ClearAppointmentFields()
        {
            textbox_CustomerVehicle.Text = string.Empty;
            textbox_VehicleType.Text = string.Empty;
            textbox_RecommendedTires.Text = string.Empty;
            textbox_TiresInStock.Text = "0";
        }

        // SAVE APPOINTMENT //////////////////////////////////////////////////////////////////////////
        private void Button_SaveAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                if (TrySaveAppointment())
                {
                    NavigateBack();
                }
            }
        }

        private bool TrySaveAppointment()
        {
            if (combobox_Customer.SelectedItem is Customer selectedCustomer &&
                datepicker_AppointmentDate.SelectedDate.HasValue &&
                combobox_AppointmentTime.SelectedItem is string selectedTime)
            {
                DateTime appointmentDateTime = datepicker_AppointmentDate.SelectedDate.Value;

                // Parse the selected time
                string[] timeParts = selectedTime.Split(' ');
                string[] hourMinute = timeParts[0].Split(':');
                int hour = int.Parse(hourMinute[0]);
                int minute = int.Parse(hourMinute[1]);

                // Adjust for PM
                if (timeParts[1] == "PM" && hour != 12)
                {
                    hour += 12;
                }
                // Adjust for AM
                if (timeParts[1] == "AM" && hour == 12)
                {
                    hour = 0;
                }

                appointmentDateTime = appointmentDateTime.AddHours(hour).AddMinutes(minute);

                if (!IsWithinBusinessHours(appointmentDateTime))
                {
                    MessageBox.Show("Appointments can only be scheduled between 8 AM and 5 PM.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (IsAppointmentOverlapping(appointmentDateTime, _existingAppointment?.Id))
                {
                    MessageBox.Show("This appointment overlaps with an existing appointment. Please choose a different time.", "Overlapping Appointment", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var appointment = _existingAppointment ?? new Appointment();
                appointment.CustomerId = selectedCustomer.Id;
                appointment.Date = appointmentDateTime.ToString("yyyy-MM-dd HH:mm");
                appointment.Description = textbox_AppointmentDescription.Text;
                appointment.SelectedTireBrand = textbox_RecommendedTires.Text;

                _dbHelper.AddOrUpdateAppointment(appointment);
                MessageBox.Show("Appointment saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _refreshCallback?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Please select a customer, date, and time for the appointment.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private bool IsAppointmentOverlapping(DateTime appointmentDateTime, int? currentAppointmentId = null)
        {
            var appointments = _dbHelper.GetAppointments();
            return appointments.Any(a =>
                a.Id != currentAppointmentId && // Exclude the current appointment being edited
                DateTime.Parse(a.Date).AddMinutes(-30) < appointmentDateTime &&
                DateTime.Parse(a.Date).AddMinutes(30) > appointmentDateTime
            );
        }
        // BACK BUTTON //////////////////////////////////////////////////////////////////////////////
        public void Button_CancelEditAppt_Click(object sender, RoutedEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("You have unsaved changes. Are you sure you want to go back without saving?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            NavigateBack();
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Button_CancelEditAppt_Click(this, new RoutedEventArgs());
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;
            Focus();
        }


        private bool HasUnsavedChanges()
        {
            if (_existingAppointment == null)
            {
                return combobox_Customer.SelectedItem != null ||
                       datepicker_AppointmentDate.SelectedDate.HasValue ||
                       combobox_AppointmentTime.SelectedItem != null ||
                       !string.IsNullOrWhiteSpace(textbox_AppointmentDescription.Text);
            }
            else
            {
                var selectedCustomer = combobox_Customer.SelectedItem as Customer;
                var selectedDate = datepicker_AppointmentDate.SelectedDate;
                var selectedTime = combobox_AppointmentTime.SelectedItem as string;

                return selectedCustomer?.Id != _existingAppointment.CustomerId ||
                       selectedDate?.Date != DateTime.Parse(_existingAppointment.Date).Date ||
                       selectedTime != DateTime.Parse(_existingAppointment.Date).ToString("hh:mm tt") ||
                       textbox_AppointmentDescription.Text != _existingAppointment.Description;
            }
        }

        private void NavigateBack()
        {
            NavigationService.Navigate(new DashboardPage(_dbHelper));
        }

        // BUSINESS HOURS AND OVERLAPPING APPOINTMENTS CHECKS //////////////////////////////////////
        private bool IsWithinBusinessHours(DateTime appointmentDateTime)
        {
            return appointmentDateTime.Hour >= 8 && appointmentDateTime.Hour < 17;
        }

        private bool IsAppointmentOverlapping(DateTime appointmentDateTime)
        {
            var appointments = _dbHelper.GetAppointments();
            return appointments.Any(a => DateTime.Parse(a.Date).AddMinutes(-30) < appointmentDateTime && DateTime.Parse(a.Date).AddMinutes(30) > appointmentDateTime);
        }


        // INPUT VALIDATION //////////////////////////////////////////////////////////////////////////
        private bool ValidateInputs()
        {
            // Implement input validation logic
            // Return false if validation fails
            return true;
        }

        // DEBUGGING /////////////////////////////////////////////////////////////////////////////////
        private void LogDebugInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[AppointmentsPage] {message}");
        }

    }
}