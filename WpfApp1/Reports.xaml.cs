using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class Reports : Page
    {
        private readonly DatabaseHelper _dbHelper;
        private List<TireReport> _reportData;

        public Reports(DatabaseHelper dbHelper)
        {
            InitializeComponent();
            _dbHelper = dbHelper;
            InitializeComboBox();
            LoadReport();
        }

        private void InitializeComboBox()
        {
            Combobox_ReportFilter.Items.Add("All Time");
            Combobox_ReportFilter.Items.Add("Last 3 Months");
            Combobox_ReportFilter.Items.Add("Last Month");
            Combobox_ReportFilter.SelectedIndex = 0;
            Combobox_ReportFilter.SelectionChanged += Combobox_ReportFilter_SelectionChanged;
        }

        private void LoadReport()
        {
            var filter = Combobox_ReportFilter.SelectedItem as string;
            DateTime? startDate = null;

            switch (filter)
            {
                case "Last Month":
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case "Last 3 Months":
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
            }

            var appointments = _dbHelper.GetAppointments();
            if (startDate.HasValue)
            {
                appointments = appointments.Where(a => DateTime.Parse(a.Date) >= startDate.Value).ToList();
            }

            var tireBrands = _dbHelper.GetTireBrands();

            _reportData = appointments
                .GroupBy(a => a.SelectedTireBrand)
                .Select(g => new TireReport
                {
                    TireBrand = g.Key,
                    TireCategory = tireBrands.FirstOrDefault(t => t.BrandName == g.Key)?.TireCategory ?? "Unknown",
                    AppointmentCount = g.Count()
                })
                .OrderByDescending(r => r.AppointmentCount)
                .ToList();

            Datagrid_Reports.ItemsSource = _reportData;
        }

        private void Combobox_ReportFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadReport();
        }

        private void Button_ExportReport_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ReportWindow(_reportData, Combobox_ReportFilter.SelectedItem as string);
            reportWindow.ShowDialog();
        }

        private void button_CancelReporting_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;
            Focus();
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                button_CancelReporting_Click(this, new RoutedEventArgs());
            }
        }
    }

    public class TireReport
    {
        public string TireBrand { get; set; }
        public string TireCategory { get; set; }
        public int AppointmentCount { get; set; }
    }
}