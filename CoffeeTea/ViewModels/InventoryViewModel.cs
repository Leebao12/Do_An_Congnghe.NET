using CoffeeTea.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoffeeTea.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly QL_CoffeeTeaEntities db = new QL_CoffeeTeaEntities();
        private ObservableCollection<InventoryItem> _allInventoryItems = new ObservableCollection<InventoryItem>();

        private ObservableCollection<InventoryItem> _inventoryItems;
        private InventoryItem _selectedInventoryItem;
        private string _inventorySearchKeyword;
        private string _selectedStatusFilter;
        private string _inventoryUpdateText;
        private string _minimumQuantityText;

        public InventoryViewModel()
        {
            StatusFilters = new ObservableCollection<string> { "Tất cả", "Còn hàng", "Sắp hết", "Hết hàng" };
            SelectedStatusFilter = "Tất cả";

            RefreshInventoryCommand = new RelayCommand(_ => LoadInventory());
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            UpdateInventoryCommand = new RelayCommand(_ => UpdateInventory(), _ => SelectedInventoryItem != null);
            UpdateMinimumQuantityCommand = UpdateInventoryCommand;

            LoadInventory();
        }

        public ObservableCollection<InventoryItem> InventoryItems
        {
            get { return _inventoryItems; }
            set
            {
                _inventoryItems = value;
                OnPropertyChanged(nameof(InventoryItems));
                RefreshSummary();
            }
        }

        public ObservableCollection<string> StatusFilters { get; private set; }

        public InventoryItem SelectedInventoryItem
        {
            get { return _selectedInventoryItem; }
            set
            {
                _selectedInventoryItem = value;
                OnPropertyChanged(nameof(SelectedInventoryItem));

                if (_selectedInventoryItem != null)
                {
                    InventoryUpdateText = _selectedInventoryItem.Quantity.ToString("0.##");
                    MinimumQuantityText = _selectedInventoryItem.MinimumQuantity.ToString("0.##");
                }
                else
                {
                    InventoryUpdateText = string.Empty;
                    MinimumQuantityText = string.Empty;
                }

                RefreshCommands();
            }
        }

        public string InventorySearchKeyword
        {
            get { return _inventorySearchKeyword; }
            set
            {
                _inventorySearchKeyword = value;
                OnPropertyChanged(nameof(InventorySearchKeyword));
                ApplyFilter();
            }
        }

        public string SelectedStatusFilter
        {
            get { return _selectedStatusFilter; }
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged(nameof(SelectedStatusFilter));
                ApplyFilter();
            }
        }

        public string MinimumQuantityText
        {
            get { return _minimumQuantityText; }
            set { _minimumQuantityText = value; OnPropertyChanged(nameof(MinimumQuantityText)); }
        }

        public string InventoryUpdateText
        {
            get { return _inventoryUpdateText; }
            set { _inventoryUpdateText = value; OnPropertyChanged(nameof(InventoryUpdateText)); }
        }

        public string TotalInventoryValueText 
        { 
            get { return (InventoryItems != null ? InventoryItems.Sum(x => x.TotalValue) : 0).ToString("N0") + " đ"; } 
        }
        public string TotalItemsText 
        { 
            get { return (InventoryItems != null ? InventoryItems.Count : 0).ToString(); } 
        }
        public string LowStockCountText 
        { 
            get { return (InventoryItems != null ? InventoryItems.Count(x => x.StatusText == "Sắp hết") : 0).ToString(); } 
        }
        public string OutOfStockCountText 
        { 
            get { return (InventoryItems != null ? InventoryItems.Count(x => x.StatusText == "Hết hàng") : 0).ToString(); } 
        }

        public ICommand RefreshInventoryCommand { get; private set; }
        public ICommand ClearFilterCommand { get; private set; }
        public ICommand UpdateInventoryCommand { get; private set; }
        public ICommand UpdateMinimumQuantityCommand { get; private set; }

        private void LoadInventory()
        {
            try
            {
                _allInventoryItems = new ObservableCollection<InventoryItem>(
                    db.NguyenLieux
                      .OrderBy(x => x.MaNguyenLieu)
                      .ToList()
                      .Select(x => new InventoryItem
                      {
                          DrinkId = x.MaNguyenLieu,
                          DrinkName = x.TenNguyenLieu,
                          CategoryName = "Nguyên liệu",
                          Quantity = x.SoLuongTon,
                          Unit = x.DonViTinh,
                          MinimumQuantity = x.MucCanhBao,
                          LastImportPrice = x.DonGiaNhapGanNhat
                      }));

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu tồn kho: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_allInventoryItems == null)
            {
                InventoryItems = new ObservableCollection<InventoryItem>();
                return;
            }

            string keyword = (InventorySearchKeyword ?? string.Empty).Trim().ToLower();
            string status = SelectedStatusFilter ?? "Tất cả";

            var query = _allInventoryItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    SafeLower(x.DrinkId).Contains(keyword) ||
                    SafeLower(x.DrinkName).Contains(keyword) ||
                    SafeLower(x.Unit).Contains(keyword));
            }

            if (status != "Tất cả")
            {
                query = query.Where(x => x.StatusText == status);
            }

            InventoryItems = new ObservableCollection<InventoryItem>(query);
        }

        private void ClearFilter()
        {
            InventorySearchKeyword = string.Empty;
            SelectedStatusFilter = "Tất cả";
            ApplyFilter();
        }

        private void UpdateInventory()
        {
            if (SelectedInventoryItem == null)
            {
                MessageBox.Show("Bạn chưa chọn mặt hàng cần cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal inventoryQuantity;
            if (!TryParseDecimal(InventoryUpdateText, out inventoryQuantity) || inventoryQuantity < 0)
            {
                MessageBox.Show("Số lượng tồn kho không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal minimumQuantity;
            if (!TryParseDecimal(MinimumQuantityText, out minimumQuantity) || minimumQuantity < 0)
            {
                MessageBox.Show("Mức tồn tối thiểu không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            

            try
            {
                string selectedId = SelectedInventoryItem.DrinkId;
                var ingredient = db.NguyenLieux.FirstOrDefault(x => x.MaNguyenLieu == selectedId);
                if (ingredient == null)
                {
                    MessageBox.Show("Không tìm thấy nguyên liệu cần cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if(ingredient.SoLuongTon < inventoryQuantity)
                {
                    MessageBox.Show("Số lượng tồn bạn nhập lớn hơn số lượng có trong kho.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ingredient.SoLuongTon = inventoryQuantity;
                ingredient.MucCanhBao = minimumQuantity;
                db.SaveChanges();

                LoadInventory();
                SelectedInventoryItem = InventoryItems.FirstOrDefault(x => x.DrinkId == selectedId);

                MessageBox.Show("Đã cập nhật tồn kho.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể cập nhật tồn kho: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static bool TryParseDecimal(string value, out decimal result)
        {
            value = (value ?? string.Empty).Trim();
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result)
                   || decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out result)
                   || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        private static string SafeLower(string value)
        {
            return (value ?? string.Empty).ToLower();
        }

        private static string GetInnermostMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }

        private void RefreshSummary()
        {
            OnPropertyChanged(nameof(TotalInventoryValueText));
            OnPropertyChanged(nameof(TotalItemsText));
            OnPropertyChanged(nameof(LowStockCountText));
            OnPropertyChanged(nameof(OutOfStockCountText));
        }

        private void RefreshCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class InventoryItem
    {
        public string DrinkId { get; set; }
        public string DrinkName { get; set; }
        public string CategoryName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal MinimumQuantity { get; set; }
        public decimal LastImportPrice { get; set; }
        public decimal TotalValue { get { return Quantity * LastImportPrice; } }

        public string StatusText
        {
            get
            {
                if (Quantity <= 0)
                {
                    return "Hết hàng";
                }

                if (Quantity <= MinimumQuantity)
                {
                    return "Sắp hết";
                }

                return "Còn hàng";
            }
        }
    }
}
