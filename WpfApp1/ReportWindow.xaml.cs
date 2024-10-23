using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public partial class ReportWindow : Window
    {
        public ReportWindow(List<TireReport> reportData, string filterType)
        {
            InitializeComponent();

            ReportTitle.Text = $"Tire Brand Report ({filterType}) - Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            ReportDataGrid.ItemsSource = reportData;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Printing functionality is not yet implemented.", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void button_CancelReporting_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_CloseReportWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}