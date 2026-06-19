using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CoffeeTea.Models;

namespace CoffeeTea.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly Action<HoaDon> _openInvoiceAction;
        private readonly Action<StatisticsReportData> _openReportAction;
        private QL_CoffeeTeaEntities _context = new QL_CoffeeTeaEntities();
        private DateTime _fromDate = DateTime.Now.Date.AddDays(-30);
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                DateTime newDate = value.Date;
                bool wasClamped = false;
                if (newDate > ToDate)
                {
                    newDate = ToDate;
                    wasClamped = true;
                }

                if (_fromDate == newDate)
                {
                    if (wasClamped)
                    {
                        OnPropertyChanged(nameof(FromDate));
                    }

                    return;
                }

                _fromDate = newDate;
                OnPropertyChanged(nameof(FromDate));
                RefreshCommands();
            }
        }

        private DateTime _toDate = DateTime.Now.Date;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                DateTime newDate = value.Date;
                bool wasClamped = false;
                if (newDate < FromDate)
                {
                    newDate = FromDate;
                    wasClamped = true;
                }

                if (_toDate == newDate)
                {
                    if (wasClamped)
                    {
                        OnPropertyChanged(nameof(ToDate));
                    }

                    return;
                }

                _toDate = newDate;
                OnPropertyChanged(nameof(ToDate));
                RefreshCommands();
            }
        }

        private ObservableCollection<HoaDon> _invoices;
        public ObservableCollection<HoaDon> Invoices
        {
            get => _invoices;
            set { _invoices = value; OnPropertyChanged(nameof(Invoices)); }
        }

        public decimal TotalRevenue => Invoices?.Sum(x => x.TongTien) ?? 0;
        public int TotalInvoices => Invoices?.Count ?? 0;
        public decimal AveragePerInvoice => TotalInvoices > 0 ? TotalRevenue / TotalInvoices : 0;

        public ICommand FilterCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand ViewInvoiceCommand { get; }

        public StatisticsViewModel(Action<HoaDon> openInvoiceAction = null, Action<StatisticsReportData> openReportAction = null)
        {
            _openInvoiceAction = openInvoiceAction;
            _openReportAction = openReportAction;
            FilterCommand = new RelayCommand(_ => LoadStatistics());
            ExportReportCommand = new RelayCommand(_ => OpenReport(), _ => TotalInvoices > 0);
            ViewInvoiceCommand = new RelayCommand(invoice => ViewInvoice(invoice as HoaDon), invoice => invoice is HoaDon);
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            DateTime fromDate = FromDate.Date;
            DateTime endOfDay = ToDate.Date.AddDays(1).AddTicks(-1);

            var result = _context.HoaDons
                .Include(h => h.Ban)
                .Include(h => h.NhanVien)
                .Where(h => h.NgayLap >= fromDate && h.NgayLap <= endOfDay && h.TrangThai == "Đã thanh toán")
                .OrderByDescending(h => h.NgayLap)
                .ToList();

            Invoices = new ObservableCollection<HoaDon>(result);
            RefreshSummary();
        }

        private void OpenReport()
        {
            if (TotalInvoices == 0)
            {
                MessageBox.Show("Không có dữ liệu hóa đơn để lập report.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_openReportAction == null)
            {
                MessageBox.Show("Chưa cấu hình màn hình xem report.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _openReportAction(BuildReportData());
        }

        private StatisticsReportData BuildReportData()
        {
            var culture = CultureInfo.GetCultureInfo("vi-VN");
            var items = Invoices
                .Select((invoice, index) => new StatisticsReportInvoiceItem
                {
                    No = index + 1,
                    InvoiceId = invoice.MaHoaDon,
                    CreatedAt = invoice.NgayLap.ToString("dd/MM/yyyy HH:mm", culture),
                    TableName = invoice.Ban?.TenBan ?? "",
                    StaffName = invoice.NhanVien?.HoTen ?? "",
                    PaymentMethod = invoice.PhuongThucTT ?? "",
                    TotalAmount = invoice.TongTien,
                    Status = invoice.TrangThai ?? ""
                })
                .ToList();

            return new StatisticsReportData
            {
                FromDate = FromDate.Date,
                ToDate = ToDate.Date,
                GeneratedAt = DateTime.Now,
                TotalRevenue = TotalRevenue,
                TotalInvoices = TotalInvoices,
                AveragePerInvoice = AveragePerInvoice,
                Items = items
            };
        }

        private void RefreshSummary()
        {
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalInvoices));
            OnPropertyChanged(nameof(AveragePerInvoice));
            RefreshCommands();
        }

        private void RefreshCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void ViewInvoice(HoaDon invoice)
        {
            if (invoice == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần xem.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _openInvoiceAction?.Invoke(invoice);
        }
    }
}
