using CoffeeTea.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoffeeTea.ViewModels
{
    public class CartItemModel : BaseViewModel
    {
        public Mon MonInfo { get; set; }
        public string TenMon
        {
            get { return !string.IsNullOrWhiteSpace(_tenMon) ? _tenMon : MonInfo?.TenMon; }
            set
            {
                _tenMon = value;
                OnPropertyChanged(nameof(TenMon));
            }
        }

        public decimal DonGia
        {
            get { return _donGia ?? (MonInfo != null ? MonInfo.DonGia : 0); }
            set
            {
                _donGia = value;
                OnPropertyChanged(nameof(DonGia));
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        private string _tenMon;
        private decimal? _donGia;

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set { _soLuong = value; OnPropertyChanged(nameof(SoLuong)); OnPropertyChanged(nameof(ThanhTien)); }
        }

        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class CategoryFilterModel : BaseViewModel
    {
        private bool _isSelected;

        public string MaDanhMuc { get; set; }

        public string TenDanhMuc { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;

                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    public class OrderViewModel : BaseViewModel
    {
        private readonly Action _orderCreatedAction;
        private readonly NhanVien _currentUser; 
        private QL_CoffeeTeaEntities _context = new QL_CoffeeTeaEntities();

        public ObservableCollection<CategoryFilterModel> Categories { get; set; }
        public ObservableCollection<Mon> MenuItems { get; set; }
        public ObservableCollection<Ban> AvailableTables { get; set; }
        public ObservableCollection<CartItemModel> CartItems { get; set; }

        private Ban _selectedTable;
        public Ban SelectedTable
        {
            get => _selectedTable;
            set { _selectedTable = value; OnPropertyChanged(nameof(SelectedTable)); UpdateCommandStates(); }
        }

        private CartItemModel _selectedCartItem;
        public CartItemModel SelectedCartItem
        {
            get => _selectedCartItem;
            set { _selectedCartItem = value; OnPropertyChanged(nameof(SelectedCartItem)); UpdateCommandStates(); }
        }

        public decimal TotalAmount => CartItems.Sum(x => x.ThanhTien);

        public ICommand FilterByCategoryCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveSelectedItemCommand { get; }
        public ICommand ClearOrderCommand { get; }
        public ICommand CreateInvoiceCommand { get; }

        public OrderViewModel(Action orderCreatedAction, NhanVien currentUser = null, Ban selectedTable = null)
        {
            _orderCreatedAction = orderCreatedAction;
            _currentUser = currentUser; 
            CartItems = new ObservableCollection<CartItemModel>();
            CartItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalAmount));
                UpdateCommandStates();
            };

            LoadData();
            SelectInitialTable(selectedTable);

            FilterByCategoryCommand = new RelayCommand(param => FilterMenu(param as string));
            AddToCartCommand = new RelayCommand(param => AddToCart(param as Mon));
            IncreaseQuantityCommand = new RelayCommand(param => ChangeQuantity(param as CartItemModel, 1), param => param is CartItemModel);
            DecreaseQuantityCommand = new RelayCommand(param => ChangeQuantity(param as CartItemModel, -1), param => CanDecreaseQuantity(param as CartItemModel));
            RemoveSelectedItemCommand = new RelayCommand(_ => RemoveSelectedItem(), _ => SelectedCartItem != null);
            ClearOrderCommand = new RelayCommand(_ => ClearOrder(), _ => CartItems.Any() || SelectedTable != null);
            CreateInvoiceCommand = new RelayCommand(_ => CreateInvoice(), _ => SelectedTable != null && CartItems.Any());
        }

        private void LoadData()
        {
            var categories = _context.DanhMucMons
                .Select(category => new CategoryFilterModel
                {
                    MaDanhMuc = category.MaDanhMuc,
                    TenDanhMuc = category.TenDanhMuc
                })
                .ToList();

            categories.Insert(0, new CategoryFilterModel { MaDanhMuc = "ALL", TenDanhMuc = "Tất cả", IsSelected = true });
            Categories = new ObservableCollection<CategoryFilterModel>(categories);
            MenuItems = new ObservableCollection<Mon>(_context.Mons.Where(m => m.TrangThai == "Đang bán").ToList());
            AvailableTables = new ObservableCollection<Ban>(_context.Bans.Where(b => b.TrangThai == "Trống").ToList());
        }

        private void SelectInitialTable(Ban selectedTable)
        {
            if (selectedTable == null) return;

            SelectedTable = AvailableTables.FirstOrDefault(b => b.MaBan == selectedTable.MaBan);
        }

        private void FilterMenu(string categoryId)
        {
            categoryId = string.IsNullOrEmpty(categoryId) ? "ALL" : categoryId;

            if (categoryId == "ALL" || string.IsNullOrEmpty(categoryId))
                MenuItems = new ObservableCollection<Mon>(_context.Mons.Where(m => m.TrangThai == "Đang bán").ToList());
            else
                MenuItems = new ObservableCollection<Mon>(_context.Mons.Where(m => m.MaDanhMuc == categoryId && m.TrangThai == "Đang bán").ToList());

            foreach (var category in Categories)
            {
                category.IsSelected = string.Equals(category.MaDanhMuc, categoryId, StringComparison.OrdinalIgnoreCase);
            }

            OnPropertyChanged(nameof(MenuItems));
        }

        private void AddToCart(Mon item)
        {
            if (item == null) return;
            var existingItem = CartItems.FirstOrDefault(c => c.MonInfo.MaMon == item.MaMon);
            if (existingItem != null)
            {
                existingItem.SoLuong++;
            }
            else
            {
                CartItems.Add(new CartItemModel { MonInfo = item, SoLuong = 1 });
            }
            OnPropertyChanged(nameof(TotalAmount));
            UpdateCommandStates();
        }

        private void ChangeQuantity(CartItemModel item, int amount)
        {
            if (item == null) return;

            int newQuantity = item.SoLuong + amount;
            if (newQuantity < 1) return;

            item.SoLuong = newQuantity;
            OnPropertyChanged(nameof(TotalAmount));
            UpdateCommandStates();
        }

        private bool CanDecreaseQuantity(CartItemModel item)
        {
            return item != null && item.SoLuong > 1;
        }

        private void RemoveSelectedItem()
        {
            if (SelectedCartItem == null) return;

            CartItems.Remove(SelectedCartItem);
            SelectedCartItem = null;
            OnPropertyChanged(nameof(TotalAmount));
            UpdateCommandStates();
        }

        private void ClearOrder()
        {
            CartItems.Clear();
            SelectedTable = null;
            SelectedCartItem = null;
            UpdateCommandStates();
        }
        private void CreateInvoice()
        {
            if (SelectedTable == null || !CartItems.Any()) return;

            try
            {
                using (var context = new QL_CoffeeTeaEntities())
                {
                    var table = context.Bans.FirstOrDefault(b => b.MaBan == SelectedTable.MaBan);
                    if (table == null)
                    {
                        MessageBox.Show("Không tìm thấy bàn đã chọn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (table.TrangThai != "Trống")
                    {
                        MessageBox.Show($"{table.TenBan} hiện không còn trống, vui lòng chọn bàn khác.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        return;
                    }

                    string invoiceId = GenerateNewInvoiceId(context);
                    var invoice = new HoaDon
                    {
                        MaHoaDon = invoiceId,
                        NgayLap = DateTime.Now,
                        MaNhanVien = _currentUser != null ? _currentUser.MaNhanVien : "NV01",
                        MaBan = table.MaBan,
                        TongTien = TotalAmount,
                        PhuongThucTT = "",
                        TrangThai = "Chưa thanh toán",
                        GhiChu = ""
                    };

                    context.HoaDons.Add(invoice);

                    int detailNumber = GetNextInvoiceDetailNumber(context);
                    foreach (var item in CartItems)
                    {
                        var detail = new ChiTietHoaDon
                        {
                            MaCTHD = "CT" + detailNumber.ToString("D5"),
                            MaHoaDon = invoiceId,
                            MaMon = item.MonInfo.MaMon,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            ThanhTien = item.ThanhTien,
                            GhiChu = ""
                        };

                        context.ChiTietHoaDons.Add(detail);
                        detailNumber++;
                    }

                    table.TrangThai = "Đang phục vụ";
                    context.SaveChanges();
                }

                MessageBox.Show("Lập hóa đơn thành công. Bàn đã chuyển sang trạng thái Đang phục vụ.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                _orderCreatedAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lập hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateNewInvoiceId(QL_CoffeeTeaEntities context)
        {
            int number = context.HoaDons.Count() + 1;
            string invoiceId;

            do
            {
                invoiceId = "HD" + number.ToString("D4");
                number++;
            }
            while (context.HoaDons.Any(h => h.MaHoaDon == invoiceId));

            return invoiceId;
        }

        private int GetNextInvoiceDetailNumber(QL_CoffeeTeaEntities context)
        {
            int number = context.ChiTietHoaDons.Count() + 1;
            string detailId = "CT" + number.ToString("D5");

            while (context.ChiTietHoaDons.Any(ct => ct.MaCTHD == detailId))
            {
                number++;
                detailId = "CT" + number.ToString("D5");
            }

            return number;
        }
        private void UpdateCommandStates()
        {
            (IncreaseQuantityCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DecreaseQuantityCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemoveSelectedItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClearOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CreateInvoiceCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

    }
}
