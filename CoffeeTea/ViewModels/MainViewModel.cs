using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CoffeeTea.Models;
using CoffeeTea.Views;

namespace CoffeeTea.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly NhanVien _authenticatedUser;
        private readonly Action _logoutAction;

        private object _currentView;
        private ICommand _dashboardCommand;
        private ICommand _drinkCommand;
        private ICommand _categoryCommand;
        private ICommand _tableManagementCommand;
        private ICommand _staffCommand;
        private ICommand _supplierCommand;
        private ICommand _orderCommand;
        private ICommand _paymentCommand;
        private ICommand _tableStatusCommand;
        private ICommand _importCommand;
        private ICommand _inventoryCommand;
        private ICommand _revenueReportCommand;
        private ICommand _profileCommand;
        private ICommand _settingsCommand;
        private ICommand _logoutCommand;

        public MainViewModel(NhanVien authenticatedUser, Action logoutAction)
        {
            _authenticatedUser = authenticatedUser;
            _logoutAction = logoutAction;

            CurrentAccountName = ResolveAccountName();
            CurrentAccountRole = ResolveRoleDisplay();
            BuildPermissionSet();

            _dashboardCommand = new RelayCommand(_ => CurrentView = new UCDashboardView(_authenticatedUser), _ => CanAccessSalesMenu);
            _drinkCommand = new RelayCommand(_ => CurrentView = new UCDrinkManagement(), _ => CanAccessCatalogMenu);
            _categoryCommand = new RelayCommand(_ => CurrentView = new UCCategoryManagement(), _ => CanAccessCatalogMenu);
            _tableManagementCommand = new RelayCommand(_ => CurrentView = new UCTableManagement(), _ => CanAccessCatalogMenu);
            _staffCommand = new RelayCommand(_ => CurrentView = new UCStaffManagement(), _ => CanAccessCatalogMenu);
            _supplierCommand = new RelayCommand(_ => CurrentView = new UCSupplierManagement(), _ => CanAccessCatalogMenu);
            _orderCommand = new RelayCommand(_ => ChuyenSangManHinhOrder(), _ => CanAccessSalesMenu);
            _paymentCommand = new RelayCommand(_ => CurrentView = new UCPayment(), _ => CanAccessSalesMenu);
            _tableStatusCommand = new RelayCommand(_ => ChuyenSangManHinhTrangThaiBan(), _ => CanAccessSalesMenu);
            _importCommand = new RelayCommand(_ => CurrentView = new UCImportReceipt(), _ => CanAccessWarehouseMenu);
            _inventoryCommand = new RelayCommand(_ => CurrentView = new UCInventory(), _ => CanAccessWarehouseMenu);
            _revenueReportCommand = new RelayCommand(_ => ChuyenSangManHinhThongKe(), _ => CanAccessStatisticsMenu);
            _profileCommand = new RelayCommand(_ => CurrentView = new UCProfile(_authenticatedUser), _ => CanAccessSystemMenu);
            _settingsCommand = new RelayCommand(_ => CurrentView = new UCSettings(_authenticatedUser), _ => CanAccessSettings);
            _logoutCommand = new RelayCommand(_ => _logoutAction?.Invoke());

            CurrentView = new UCDashboardView(_authenticatedUser);
        }

        public string CurrentAccountName { get; private set; }

        public string CurrentAccountRole { get; private set; }

        public bool CanAccessCatalogMenu { get; private set; }

        public bool CanAccessSalesMenu { get; private set; }

        public bool CanAccessWarehouseMenu { get; private set; }

        public bool CanAccessStatisticsMenu { get; private set; }

        public bool CanAccessSystemMenu { get; private set; }

        public bool CanAccessSettings { get; private set; }

        public object CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                if (_currentView == value)
                {
                    return;
                }

                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public NhanVien AuthenticatedUser
        {
            get { return _authenticatedUser; }
        }

        public ICommand DashboardCommand
        {
            get { return _dashboardCommand; }
        }

        public ICommand DrinkCommand
        {
            get { return _drinkCommand; }
        }

        public ICommand CategoryCommand
        {
            get { return _categoryCommand; }
        }

        public ICommand TableManagementCommand
        {
            get { return _tableManagementCommand; }
        }

        public ICommand StaffCommand
        {
            get { return _staffCommand; }
        }

        public ICommand SupplierCommand
        {
            get { return _supplierCommand; }
        }

        public ICommand OrderCommand
        {
            get { return _orderCommand; }
        }

        public ICommand PaymentCommand
        {
            get { return _paymentCommand; }
        }

        public ICommand TableStatusCommand
        {
            get { return _tableStatusCommand; }
        }

        public ICommand ImportCommand
        {
            get { return _importCommand; }
        }

        public ICommand InventoryCommand
        {
            get { return _inventoryCommand; }
        }

        public ICommand RevenueReportCommand
        {
            get { return _revenueReportCommand; }
        }

        public ICommand ProfileCommand
        {
            get { return _profileCommand; }
        }

        public ICommand SettingsCommand
        {
            get { return _settingsCommand; }
        }

        public ICommand LogoutCommand
        {
            get { return _logoutCommand; }
        }

        private string ResolveAccountName()
        {
            if (_authenticatedUser == null || string.IsNullOrWhiteSpace(_authenticatedUser.HoTen))
            {
                return "Tài khoản không xác định";
            }

            return _authenticatedUser.HoTen.Trim();
        }

        private string ResolveRoleDisplay()
        {
            string roleName = _authenticatedUser?.VaiTro != null ? _authenticatedUser.VaiTro.TenVaiTro : null;
            string roleCode = _authenticatedUser != null ? _authenticatedUser.MaVaiTro : null;

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                return roleName.Trim();
            }

            if (string.Equals(roleCode, "VT01", StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }

            if (string.Equals(roleCode, "VT02", StringComparison.OrdinalIgnoreCase))
            {
                return "Quản lý";
            }

            if (string.Equals(roleCode, "VT03", StringComparison.OrdinalIgnoreCase))
            {
                return "Nhân viên";
            }

            return "Không xác định";
        }

        private void BuildPermissionSet()
        {
            bool isAdmin = string.Equals(_authenticatedUser?.MaVaiTro, "VT01", StringComparison.OrdinalIgnoreCase);
            bool isManager = string.Equals(_authenticatedUser?.MaVaiTro, "VT02", StringComparison.OrdinalIgnoreCase);
            bool isStaff = string.Equals(_authenticatedUser?.MaVaiTro, "VT03", StringComparison.OrdinalIgnoreCase);

            CanAccessCatalogMenu = isAdmin || isManager;
            CanAccessSalesMenu = true;
            CanAccessWarehouseMenu = isAdmin || isManager || isStaff;
            CanAccessStatisticsMenu = isAdmin || isManager;
            CanAccessSystemMenu = true;
            CanAccessSettings = true;
        }

        private void QuayVeManHinhOrder()
        {
            var orderVM = new OrderViewModel(ChuyenSangManHinhTrangThaiBan, _authenticatedUser);
            var orderView = new UCOrder { DataContext = orderVM };
            CurrentView = orderView;
        }

        private void ChuyenSangManHinhOrder(Ban selectedTable = null)
        {
            var orderVM = new OrderViewModel(ChuyenSangManHinhTrangThaiBan, _authenticatedUser, selectedTable);
            var orderView = new UCOrder { DataContext = orderVM };
            CurrentView = orderView;
        }

        private void ChuyenSangManHinhTrangThaiBan()
        {
            var tableStatusVM = new TableStatusViewModel(ChuyenSangManHinhOrder, ChuyenSangThanhToanTheoBan);
            CurrentView = new UCTableStatus(tableStatusVM);
        }

        private void ChuyenSangThanhToanTheoBan(Ban table)
        {
            InvoiceDetailModel invoiceData = TaoHoaDonThanhToanTuBan(table);
            if (invoiceData == null)
            {
                MessageBox.Show($"Không tìm thấy hóa đơn chưa thanh toán của {table.TenBan}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var paymentVM = new PaymentViewModel(invoiceData, ChuyenSangManHinhTrangThaiBan);
            CurrentView = new UCPayment { DataContext = paymentVM };
        }

        private InvoiceDetailModel TaoHoaDonThanhToanTuBan(Ban table)
        {
            if (table == null) return null;

            using (var context = new QL_CoffeeTeaEntities())
            {
                var invoice = context.HoaDons
                    .Include(h => h.Ban)
                    .Include(h => h.ChiTietHoaDons.Select(ct => ct.Mon))
                    .Where(h => h.MaBan == table.MaBan && h.TrangThai == "Chưa thanh toán")
                    .OrderByDescending(h => h.NgayLap)
                    .FirstOrDefault();

                if (invoice == null) return null;

                return TaoChiTietHoaDonThanhToan(invoice, table.TenBan);
            }
        }

        private void ChuyenSangThanhToanTheoHoaDon(HoaDon selectedInvoice)
        {
            if (selectedInvoice == null) return;

            object previousStatisticsView = CurrentView;
            InvoiceDetailModel invoiceData = TaoHoaDonThanhToanTuMaHoaDon(selectedInvoice.MaHoaDon);
            if (invoiceData == null)
            {
                MessageBox.Show("Không tìm thấy dữ liệu chi tiết của hóa đơn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var paymentVM = new PaymentViewModel(invoiceData, () => QuayVeManHinhThongKe(previousStatisticsView));
            CurrentView = new UCPayment { DataContext = paymentVM };
        }

        private void QuayVeManHinhThongKe(object previousStatisticsView)
        {
            if (previousStatisticsView != null)
            {
                CurrentView = previousStatisticsView;
                return;
            }

            ChuyenSangManHinhThongKe();
        }

        private InvoiceDetailModel TaoHoaDonThanhToanTuMaHoaDon(string maHoaDon)
        {
            if (string.IsNullOrWhiteSpace(maHoaDon)) return null;

            using (var context = new QL_CoffeeTeaEntities())
            {
                var invoice = context.HoaDons
                    .Include(h => h.Ban)
                    .Include(h => h.ChiTietHoaDons.Select(ct => ct.Mon))
                    .FirstOrDefault(h => h.MaHoaDon == maHoaDon);

                if (invoice == null) return null;

                return TaoChiTietHoaDonThanhToan(invoice);
            }
        }

        private InvoiceDetailModel TaoChiTietHoaDonThanhToan(HoaDon invoice, string fallbackTableName = null)
        {
            var items = invoice.ChiTietHoaDons
                .Select(detail => new CartItemModel
                {
                    MonInfo = detail.Mon,
                    TenMon = detail.Mon != null ? detail.Mon.TenMon : detail.MaMon,
                    DonGia = detail.DonGia,
                    SoLuong = detail.SoLuong
                })
                .ToList();

            decimal total = invoice.TongTien > 0 ? invoice.TongTien : items.Sum(item => item.ThanhTien);

            return new InvoiceDetailModel
            {
                MaHoaDon = invoice.MaHoaDon,
                TenBan = invoice.Ban != null ? invoice.Ban.TenBan : fallbackTableName,
                MaBan = invoice.MaBan,
                MaNhanVien = invoice.MaNhanVien,
                PhuongThucTT = invoice.PhuongThucTT,
                TrangThai = invoice.TrangThai,
                NgayLap = invoice.NgayLap,
                TongTien = total,
                Items = new ObservableCollection<CartItemModel>(items)
            };
        }

        private void ChuyenSangManHinhThongKe()
        {
            CurrentView = new UCStoreStatistics(ChuyenSangThanhToanTheoHoaDon);
        }
    }
}
