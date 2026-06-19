using CoffeeTea.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoffeeTea.ViewModels
{
    public class SupplierViewModel : BaseViewModel
    {
        private readonly QL_CoffeeTeaEntities db = new QL_CoffeeTeaEntities();
        private ObservableCollection<SupplierItem> _allSuppliers = new ObservableCollection<SupplierItem>();

        private ObservableCollection<SupplierItem> _suppliers;
        private SupplierItem _selectedSupplier;
        private string _supplierId;
        private string _supplierName;
        private string _phone;
        private string _email;
        private string _address;
        private string _note;
        private bool _isActive;
        private string _supplierSearchKeyword;

        public SupplierViewModel()
        {
            Suppliers = new ObservableCollection<SupplierItem>();

            AddSupplierCommand = new RelayCommand(_ => AddSupplier(), _ => CanSaveSupplier());
            UpdateSupplierCommand = new RelayCommand(_ => UpdateSupplier(), _ => SelectedSupplier != null && CanSaveSupplier());
            DeleteSupplierCommand = new RelayCommand(_ => DeleteSupplier(), _ => SelectedSupplier != null);
            ClearSupplierCommand = new RelayCommand(_ => ClearForm());
            SearchSupplierCommand = new RelayCommand(_ => ApplySearch());

            LoadSuppliers();
            ClearForm();
        }

        public ObservableCollection<SupplierItem> Suppliers
        {
            get { return _suppliers; }
            set
            {
                _suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
                RefreshSummary();
            }
        }

        public SupplierItem SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged(nameof(SelectedSupplier));

                if (_selectedSupplier != null)
                {
                    SupplierId = _selectedSupplier.SupplierId;
                    SupplierName = _selectedSupplier.SupplierName;
                    Phone = _selectedSupplier.Phone;
                    Email = _selectedSupplier.Email;
                    Address = _selectedSupplier.Address;
                    Note = _selectedSupplier.Note;
                    IsActive = _selectedSupplier.IsActive;
                }

                RefreshCommands();
            }
        }

        public string SupplierId
        {
            get { return _supplierId; }
            set { 
                _supplierId = value; 
                OnPropertyChanged(nameof(SupplierId)); 
            }
        }

        public string SupplierName
        {
            get { return _supplierName; }
            set { 
                _supplierName = value; 
                OnPropertyChanged(nameof(SupplierName)); 
                RefreshCommands(); 
            }
        }

        public string Phone
        {
            get { return _phone; }
            set { 
                _phone = value; 
                OnPropertyChanged(nameof(Phone)); 
                RefreshCommands(); 
            }
        }

        public string Email
        {
            get { return _email; }
            set { 
                _email = value; 
                OnPropertyChanged(nameof(Email)); 
                RefreshCommands(); 
            }
        }

        public string Address
        {
            get { return _address; }
            set { 
                _address = value; 
                OnPropertyChanged(nameof(Address)); 
                RefreshCommands(); 
            }
        }

        public string Note
        {
            get { return _note; }
            set { 
                _note = value; 
                OnPropertyChanged(nameof(Note)); 
                RefreshCommands(); 
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { 
                _isActive = value; 
                OnPropertyChanged(nameof(IsActive)); 
                RefreshCommands(); 
            }
        }

        public string SupplierSearchKeyword
        {
            get { return _supplierSearchKeyword; }
            set
            {
                _supplierSearchKeyword = value;
                OnPropertyChanged(nameof(SupplierSearchKeyword));
                ApplySearch();
            }
        }

        public string SupplierCountText 
        { 
            get { return Suppliers != null ? Suppliers.Count.ToString() : "0"; } 
        }
        public string ActiveSupplierCountText 
        { 
            get { return Suppliers != null ? Suppliers.Count(x => x.IsActive).ToString() : "0"; } 
        }
        public string PauseSupplierCountText 
        { 
            get { return Suppliers != null ? Suppliers.Count(x => !x.IsActive).ToString() : "0"; } 
        }

        public ICommand AddSupplierCommand { get; private set; }
        public ICommand UpdateSupplierCommand { get; private set; }
        public ICommand DeleteSupplierCommand { get; private set; }
        public ICommand ClearSupplierCommand { get; private set; }
        public ICommand SearchSupplierCommand { get; private set; }

        private void LoadSuppliers()
        {
            try
            {
                _allSuppliers = new ObservableCollection<SupplierItem>(
                    db.NhaCungCaps
                      .OrderBy(x => x.MaNCC)
                      .ToList()
                      .Select(x => SupplierItem.FromEntity(x)));

                ApplySearch();
                RefreshSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách nhà cung cấp: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySearch()
        {
            string keyword = (SupplierSearchKeyword ?? string.Empty).Trim().ToLower();

            var query = _allSuppliers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    SafeLower(x.SupplierId).Contains(keyword) ||
                    SafeLower(x.SupplierName).Contains(keyword) ||
                    SafeLower(x.Phone).Contains(keyword) ||
                    SafeLower(x.Email).Contains(keyword) ||
                    SafeLower(x.Address).Contains(keyword));
            }

            Suppliers = new ObservableCollection<SupplierItem>(query);
            RefreshSummary();
        }

        private void AddSupplier()
        {
            string validationMessage;
            if (!ValidateSupplierForm(out validationMessage))
            {
                MessageBox.Show(validationMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string newId = GenerateNewSupplierId();
                SupplierId = newId;

                if (db.NhaCungCaps.Any(x => x.MaNCC == newId))
                {
                    MessageBox.Show("Mã nhà cung cấp đã tồn tại. Bấm Làm mới rồi thử lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string normalizedName = SupplierName.Trim();
                if (db.NhaCungCaps.Any(x => x.TenNCC == normalizedName))
                {
                    MessageBox.Show("Tên nhà cung cấp đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var supplier = new NhaCungCap
                {
                    MaNCC = newId,
                    TenNCC = normalizedName,
                    SoDienThoai = NormalizeText(Phone, 15),
                    Email = NormalizeText(Email, 100),
                    DiaChi = NormalizeText(Address, 200),
                    GhiChu = BuildStoredNote()
                };

                db.NhaCungCaps.Add(supplier);
                db.SaveChanges();

                LoadSuppliers();
                ClearForm();

                MessageBox.Show("Đã thêm nhà cung cấp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể thêm nhà cung cấp: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSupplier()
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Bạn chưa chọn nhà cung cấp cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string validationMessage;
            if (!ValidateSupplierForm(out validationMessage))
            {
                MessageBox.Show(validationMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string id = SelectedSupplier.SupplierId;
                var supplier = db.NhaCungCaps.FirstOrDefault(x => x.MaNCC == id);
                if (supplier == null)
                {
                    MessageBox.Show("Không tìm thấy nhà cung cấp cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string normalizedName = SupplierName.Trim();
                if (db.NhaCungCaps.Any(x => x.MaNCC != id && x.TenNCC == normalizedName))
                {
                    MessageBox.Show("Tên nhà cung cấp đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                supplier.TenNCC = normalizedName;
                supplier.SoDienThoai = NormalizeText(Phone, 15);
                supplier.Email = NormalizeText(Email, 100);
                supplier.DiaChi = NormalizeText(Address, 200);
                supplier.GhiChu = BuildStoredNote();

                db.SaveChanges();

                LoadSuppliers();
                SelectedSupplier = Suppliers.FirstOrDefault(x => x.SupplierId == id);

                MessageBox.Show("Đã sửa nhà cung cấp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể sửa nhà cung cấp: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSupplier()
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Bạn chưa chọn nhà cung cấp cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string id = SelectedSupplier.SupplierId;
                var supplier = db.NhaCungCaps.FirstOrDefault(x => x.MaNCC == id);
                if (supplier == null)
                {
                    MessageBox.Show("Không tìm thấy nhà cung cấp cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool usedInReceipt = db.PhieuNhaps.Any(x => x.MaNCC == id);
                if (usedInReceipt)
                {
                    var result = MessageBox.Show(
                        "Nhà cung cấp này đã có phiếu nhập nên không thể xóa cứng. Bạn có muốn chuyển sang trạng thái Tạm ngưng không?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    supplier.GhiChu = MarkPaused(supplier.GhiChu);
                    db.SaveChanges();
                }
                else
                {
                    var result = MessageBox.Show("Bạn chắc chắn muốn xóa nhà cung cấp này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    db.NhaCungCaps.Remove(supplier);
                    db.SaveChanges();
                }

                LoadSuppliers();
                ClearForm();

                MessageBox.Show("Đã xử lý nhà cung cấp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa nhà cung cấp: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            _selectedSupplier = null;
            OnPropertyChanged(nameof(SelectedSupplier));

            SupplierId = GenerateNewSupplierId();
            SupplierName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            Note = string.Empty;
            IsActive = true;
            RefreshCommands();
        }

        private bool CanSaveSupplier()
        {
            return !string.IsNullOrWhiteSpace(SupplierName);
        }

        private bool ValidateSupplierForm(out string message)
        {
            if (string.IsNullOrWhiteSpace(SupplierName))
            {
                message = "Bạn chưa nhập tên nhà cung cấp.";
                return false;
            }

            if (SupplierName.Trim().Length > 150)
            {
                message = "Tên nhà cung cấp không được quá 150 ký tự.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Phone) && Phone.Trim().Length > 15)
            {
                message = "Số điện thoại không được quá 15 ký tự.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Phone) && !Regex.IsMatch(Phone.Trim(), @"^[0-9+\-\s().]+$"))
            {
                message = "Số điện thoại không hợp lệ.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Email) && Email.Trim().Length > 100)
            {
                message = "Email không được quá 100 ký tự.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                message = "Email không hợp lệ.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Address) && Address.Trim().Length > 200)
            {
                message = "Địa chỉ không được quá 200 ký tự.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private string GenerateNewSupplierId()
        {
            int max = 0;
            foreach (var id in db.NhaCungCaps.Select(x => x.MaNCC).ToList())
            {
                int number = ExtractNumber(id);
                if (number > max)
                {
                    max = number;
                }
            }

            return "NCC" + (max + 1).ToString("000");
        }

        private string BuildStoredNote()
        {
            string value = Note ?? string.Empty;
            value = value.Replace("[Tạm ngưng]", string.Empty).Trim();

            if (!IsActive)
            {
                value = ("[Tạm ngưng] " + value).Trim();
            }

            return NormalizeText(value, 255);
        }

        private static string MarkPaused(string currentNote)
        {
            string value = (currentNote ?? string.Empty).Replace("[Tạm ngưng]", string.Empty).Trim();
            value = ("[Tạm ngưng] " + value).Trim();
            return NormalizeText(value, 255);
        }

        private static string NormalizeText(string value, int maxLength)
        {
            value = (value ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                return null;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static string SafeLower(string value)
        {
            return (value ?? string.Empty).ToLower();
        }

        private static int ExtractNumber(string value)
        {
            string digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            int number;
            return int.TryParse(digits, out number) ? number : 0;
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
            OnPropertyChanged(nameof(SupplierCountText));
            OnPropertyChanged(nameof(ActiveSupplierCountText));
            OnPropertyChanged(nameof(PauseSupplierCountText));
        }

        private void RefreshCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class SupplierItem
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }
        public string StatusText { get { return IsActive ? "Đang hợp tác" : "Tạm ngưng"; } }

        public static SupplierItem FromEntity(NhaCungCap supplier)
        {
            string note = supplier.GhiChu ?? string.Empty;
            bool paused = note.Contains("[Tạm ngưng]");

            return new SupplierItem
            {
                SupplierId = supplier.MaNCC,
                SupplierName = supplier.TenNCC,
                Phone = supplier.SoDienThoai,
                Email = supplier.Email,
                Address = supplier.DiaChi,
                Note = note.Replace("[Tạm ngưng]", string.Empty).Trim(),
                IsActive = !paused
            };
        }
    }

}
