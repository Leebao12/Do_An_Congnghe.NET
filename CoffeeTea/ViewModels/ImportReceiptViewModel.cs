using CoffeeTea.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoffeeTea.ViewModels
{
    public class ImportReceiptViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly QL_CoffeeTeaEntities db = new QL_CoffeeTeaEntities();

        private ObservableCollection<SupplierItem> _suppliers;
        private ObservableCollection<IngredientLookupItem> _drinkItems;
        private ObservableCollection<ImportDetailItem> _importDetails;
        private ObservableCollection<ImportReceiptItem> _importReceipts;

        private SupplierItem _selectedSupplier;
        private IngredientLookupItem _selectedDrink;
        private ImportReceiptItem _selectedReceipt;
        private string _currentReceiptId;
        private DateTime? _importDate;
        private string _quantityText;
        private string _importPriceText;
        private string _note;

        public ImportReceiptViewModel()
        {
            ImportDate = DateTime.Today;
            ImportDetails = new ObservableCollection<ImportDetailItem>();

            AddDetailCommand = new RelayCommand(_ => AddDetail(), _ => SelectedDrink != null);
            RemoveDetailCommand = new RelayCommand(RemoveDetail, p => p is ImportDetailItem);
            SaveReceiptCommand = new RelayCommand(_ => SaveReceipt(), _ => CanSaveReceipt());
            ClearReceiptCommand = new RelayCommand(_ => ClearReceipt());
            RefreshReceiptCommand = new RelayCommand(_ => RefreshAllData());

            RefreshAllData();
            ClearReceipt();
        }

        public string CurrentReceiptId
        {
            get { return _currentReceiptId; }
            set { 
                _currentReceiptId = value; 
                OnPropertyChanged(nameof(CurrentReceiptId)); 
            }
        }

        public ObservableCollection<SupplierItem> Suppliers
        {
            get { return _suppliers; }
            set { 
                _suppliers = value; 
                OnPropertyChanged(nameof(Suppliers)); 
            }
        }

        public SupplierItem SelectedSupplier
        {
            get { return _selectedSupplier; }
            set { 
                _selectedSupplier = value; 
                OnPropertyChanged(nameof(SelectedSupplier)); 
                RefreshCommands(); 
            }
        }

        public DateTime? ImportDate
        {
            get { return _importDate; }
            set { _importDate = value; 
                OnPropertyChanged(nameof(ImportDate)); 
                RefreshCommands(); 
            }
        }

        public ObservableCollection<IngredientLookupItem> DrinkItems
        {
            get { return _drinkItems; }
            set { _drinkItems = value; 
                OnPropertyChanged(nameof(DrinkItems)); 
            }
        }

        public IngredientLookupItem SelectedDrink
        {
            get { return _selectedDrink; }
            set
            {
                _selectedDrink = value;
                OnPropertyChanged(nameof(SelectedDrink));

                if (_selectedDrink != null)
                {
                    ImportPriceText = _selectedDrink.ImportPrice.ToString("0.##");
                }

                RefreshCommands();
            }
        }

        public string QuantityText
        {
            get { return _quantityText; }
            set { 
                _quantityText = value; 
                OnPropertyChanged(nameof(QuantityText)); 
                RefreshCommands(); 
            }
        }

        public string ImportPriceText
        {
            get { return _importPriceText; }
            set { 
                _importPriceText = value; 
                OnPropertyChanged(nameof(ImportPriceText)); 
                RefreshCommands(); 
            }
        }

        public string Note
        {
            get { return _note; }
            set { 
                _note = value; 
                OnPropertyChanged(nameof(Note)); 
            }
        }

        public ObservableCollection<ImportDetailItem> ImportDetails
        {
            get { return _importDetails; }
            set
            {
                _importDetails = value;
                OnPropertyChanged(nameof(ImportDetails));
                RefreshDetailSummary();
                RefreshCommands();
            }
        }

        public ObservableCollection<ImportReceiptItem> ImportReceipts
        {
            get { return _importReceipts; }
            set
            {
                _importReceipts = value;
                OnPropertyChanged(nameof(ImportReceipts));
                OnPropertyChanged(nameof(ReceiptCountText));
            }
        }

        public ImportReceiptItem SelectedReceipt
        {
            get { return _selectedReceipt; }
            set { _selectedReceipt = value; 
                OnPropertyChanged(nameof(SelectedReceipt)); 
            }
        }

        public string DetailCountText 
        { 
            get { return ImportDetails != null ? ImportDetails.Count.ToString() : "0"; } 
        }
        public string TotalAmountText 
        { 
            get { return TotalAmount.ToString("N0") + " đ"; } 
        }
        public string ReceiptCountText 
        { 
            get { return ImportReceipts != null ? ImportReceipts.Count.ToString() : "0"; } 
        }

        public ICommand AddDetailCommand { get; private set; }
        public ICommand RemoveDetailCommand { get; private set; }
        public ICommand SaveReceiptCommand { get; private set; }
        public ICommand ClearReceiptCommand { get; private set; }
        public ICommand RefreshReceiptCommand { get; private set; }

        public string Error
        {
            get { return string.Empty; }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(ImportDate))
                {
                    return ValidateImportDate();
                }

                return string.Empty;
            }
        }

        private decimal TotalAmount
        {
            get { return ImportDetails != null ? ImportDetails.Sum(x => x.LineTotal) : 0; }
        }

        private void RefreshAllData()
        {
            try
            {
                LoadLookupData();
                LoadReceipts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu phiếu nhập: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLookupData()
        {
            Suppliers = new ObservableCollection<SupplierItem>(
                db.NhaCungCaps
                  .OrderBy(x => x.MaNCC)
                  .ToList()
                  .Select(x => SupplierItem.FromEntity(x))
                  .Where(x => x.IsActive));

            DrinkItems = new ObservableCollection<IngredientLookupItem>(
                db.NguyenLieux
                  .OrderBy(x => x.MaNguyenLieu)
                  .ToList()
                  .Select(x => new IngredientLookupItem
                  {
                      DrinkId = x.MaNguyenLieu,
                      DrinkName = x.TenNguyenLieu,
                      Unit = x.DonViTinh,
                      ImportPrice = x.DonGiaNhapGanNhat
                  }));
        }

        private void LoadReceipts()
        {
            ImportReceipts = new ObservableCollection<ImportReceiptItem>(
                db.PhieuNhaps
                  .Include("NhaCungCap")
                  .Include("ChiTietPhieuNhaps")
                  .OrderByDescending(x => x.NgayNhap)
                  .Take(50)
                  .ToList()
                  .Select(x => new ImportReceiptItem
                  {
                      ReceiptId = x.MaPhieuNhap,
                      ImportDate = x.NgayNhap,
                      SupplierName = x.NhaCungCap != null ? x.NhaCungCap.TenNCC : x.MaNCC,
                      DetailCount = x.ChiTietPhieuNhaps != null ? x.ChiTietPhieuNhaps.Count : 0,
                      TotalAmount = x.TongTien
                  }));
        }

        private void AddDetail()
        {
            if (SelectedDrink == null)
            {
                MessageBox.Show("Bạn chưa chọn nguyên liệu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal quantity;
            decimal price;

            if (!TryParseDecimal(QuantityText, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng nhập không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseDecimal(ImportPriceText, out price) || price < 0)
            {
                MessageBox.Show("Đơn giá nhập không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existed = ImportDetails.FirstOrDefault(x => x.DrinkId == SelectedDrink.DrinkId);
            if (existed != null)
            {
                existed.Quantity += quantity;
                existed.ImportPrice = price;
            }
            else
            {
                ImportDetails.Add(new ImportDetailItem
                {
                    LineNo = ImportDetails.Count + 1,
                    DrinkId = SelectedDrink.DrinkId,
                    DrinkName = SelectedDrink.DrinkName,
                    Unit = SelectedDrink.Unit,
                    Quantity = quantity,
                    ImportPrice = price
                });
            }

            ReindexDetails();
            QuantityText = string.Empty;
            RefreshDetailSummary();
        }

        private void RemoveDetail(object parameter)
        {
            ImportDetailItem detail = parameter as ImportDetailItem;
            if (detail == null)
            {
                return;
            }

            ImportDetails.Remove(detail);
            ReindexDetails();
            RefreshDetailSummary();
        }

        private bool CanSaveReceipt()
        {
            return SelectedSupplier != null
                   && ImportDetails != null
                   && ImportDetails.Count > 0
                   && string.IsNullOrWhiteSpace(ValidateImportDate());
        }

        private void SaveReceipt()
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Bạn chưa chọn nhà cung cấp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ImportDate == null)
            {
                MessageBox.Show("Bạn chưa chọn ngày nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string importDateValidationMessage = ValidateImportDate();
            if (!string.IsNullOrWhiteSpace(importDateValidationMessage))
            {
                MessageBox.Show(importDateValidationMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ImportDetails == null || ImportDetails.Count == 0)
            {
                MessageBox.Show("Phiếu nhập chưa có chi tiết.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    string receiptId = GenerateNewReceiptId();
                    CurrentReceiptId = receiptId;

                    var receipt = new PhieuNhap
                    {
                        MaPhieuNhap = receiptId,
                        NgayNhap = ImportDate.Value,
                        MaNCC = SelectedSupplier.SupplierId,
                        MaNhanVien = ResolveEmployeeId(),
                        TongTien = TotalAmount,
                        GhiChu = Truncate(Note, 255)
                    };

                    db.PhieuNhaps.Add(receipt);

                    int detailIndex = 0;
                    foreach (var item in ImportDetails)
                    {
                        detailIndex++;

                        db.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                        {
                            MaCTPN = GenerateNewDetailId(detailIndex),
                            MaPhieuNhap = receiptId,
                            MaNguyenLieu = item.DrinkId,
                            SoLuong = item.Quantity,
                            DonGiaNhap = item.ImportPrice
                        });

                        var ingredient = db.NguyenLieux.FirstOrDefault(x => x.MaNguyenLieu == item.DrinkId);
                        if (ingredient == null)
                        {
                            throw new InvalidOperationException("Không tìm thấy nguyên liệu: " + item.DrinkName);
                        }

                        ingredient.SoLuongTon += item.Quantity;
                        ingredient.DonGiaNhapGanNhat = item.ImportPrice;
                    }

                    db.SaveChanges();
                    transaction.Commit();
                }

                RefreshAllData();
                ClearReceipt();

                MessageBox.Show("Đã lưu phiếu nhập và cập nhật tồn kho.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu phiếu nhập: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearReceipt()
        {
            CurrentReceiptId = GenerateNewReceiptId();
            ImportDate = DateTime.Today;
            SelectedSupplier = null;
            SelectedDrink = null;
            QuantityText = string.Empty;
            ImportPriceText = string.Empty;
            Note = string.Empty;
            if (ImportDetails == null)
            {
                ImportDetails = new ObservableCollection<ImportDetailItem>();
            }
            else
            {
                ImportDetails.Clear();
            }
            RefreshDetailSummary();
            RefreshCommands();
        }

        private string ValidateImportDate()
        {
            if (!ImportDate.HasValue)
            {
                return "Bạn chưa chọn ngày nhập.";
            }

            if (ImportDate.Value.Date > DateTime.Today)
            {
                return "Ngày nhập không được lớn hơn ngày hiện tại.";
            }

            return string.Empty;
        }

        private string ResolveEmployeeId()
        {
            string employeeId = db.NhanViens
                .Where(x => x.TrangThai == "Đang làm")
                .Select(x => x.MaNhanVien)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                employeeId = db.NhanViens.Select(x => x.MaNhanVien).FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new InvalidOperationException("Chưa có nhân viên trong CSDL để gán cho phiếu nhập.");
            }

            return employeeId;
        }

        private string GenerateNewReceiptId()
        {
            int max = 0;
            foreach (var id in db.PhieuNhaps.Select(x => x.MaPhieuNhap).ToList())
            {
                int number = ExtractNumber(id);
                if (number > max)
                {
                    max = number;
                }
            }

            return "PN" + (max + 1).ToString("0000");
        }

        private string GenerateNewDetailId(int offset)
        {
            int max = 0;
            foreach (var id in db.ChiTietPhieuNhaps.Select(x => x.MaCTPN).ToList())
            {
                int number = ExtractNumber(id);
                if (number > max)
                {
                    max = number;
                }
            }

            return "CTPN" + (max + offset).ToString("000");
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            value = (value ?? string.Empty).Trim();
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result)
                   || decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out result)
                   || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        private static int ExtractNumber(string value)
        {
            string digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            int number;
            return int.TryParse(digits, out number) ? number : 0;
        }

        private static string Truncate(string value, int maxLength)
        {
            value = value ?? string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static string GetInnermostMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }

        private void ReindexDetails()
        {
            for (int i = 0; i < ImportDetails.Count; i++)
            {
                ImportDetails[i].LineNo = i + 1;
            }

            ImportDetails = new ObservableCollection<ImportDetailItem>(ImportDetails);
        }

        private void RefreshDetailSummary()
        {
            OnPropertyChanged(nameof(DetailCountText));
            OnPropertyChanged(nameof(TotalAmountText));
        }

        private void RefreshCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class IngredientLookupItem
    {
        public string DrinkId { get; set; }
        public string DrinkName { get; set; }
        public string Unit { get; set; }
        public decimal ImportPrice { get; set; }
    }

    public class ImportDetailItem : BaseViewModel
    {
        private int _lineNo;
        private decimal _quantity;
        private decimal _importPrice;

        public int LineNo
        {
            get { return _lineNo; }
            set { _lineNo = value; OnPropertyChanged(nameof(LineNo)); }
        }

        public string DrinkId { get; set; }
        public string DrinkName { get; set; }
        public string Unit { get; set; }

        public decimal Quantity
        {
            get { return _quantity; }
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(LineTotal)); }
        }

        public decimal ImportPrice
        {
            get { return _importPrice; }
            set { _importPrice = value; OnPropertyChanged(nameof(ImportPrice)); OnPropertyChanged(nameof(LineTotal)); }
        }

        public decimal LineTotal { get { return Quantity * ImportPrice; } }
    }

    public class ImportReceiptItem
    {
        public string ReceiptId { get; set; }
        public DateTime ImportDate { get; set; }
        public string SupplierName { get; set; }
        public int DetailCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
