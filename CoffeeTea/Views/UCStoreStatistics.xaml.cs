using CoffeeTea.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeTea.Views
{
    /// <summary>
    /// Interaction logic for UCStoreStatistics.xaml
    /// </summary>
    public partial class UCStoreStatistics : UserControl
    {
        public UCStoreStatistics()
            : this(null)
        {
        }

        public UCStoreStatistics(Action<CoffeeTea.Models.HoaDon> openInvoiceAction)
        {
            InitializeComponent();
            this.DataContext = new StatisticsViewModel(openInvoiceAction, OpenReportWindow);
        }

        private void OpenReportWindow(StatisticsReportData reportData)
        {
            var window = new StoreStatisticsReportWindow(reportData)
            {
                Owner = Window.GetWindow(this)
            };

            window.ShowDialog();
        }
    }
}
