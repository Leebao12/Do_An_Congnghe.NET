using CoffeeTea.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CoffeeTea.ViewModels
{
    public class StaffViewModel : BaseViewModel, IDataErrorInfo
    {
        private const string DefaultAvatarPath = "Images/Employees/default-avatar.png";

        private readonly QL_CoffeeTeaEntities db = new QL_CoffeeTeaEntities();
        private ObservableCollection<StaffItem> _allStaffs = new ObservableCollection<StaffItem>();

        private ObservableCollection<StaffItem> _staffs;
        private ObservableCollection<VaiTro> _roles;
        private StaffItem _selectedStaff;
        private string _employeeId;
        private string _fullName;
        private string _gender;
        private DateTime? _birthDate;
        private string _phone;
        private string _email;
        private string _address;
        private string _username;
        private string _password;
        private VaiTro _selectedRole;
        private DateTime? _startDate;
        private string _salaryText;
        private string _status;
        private string _searchText;

        public StaffViewModel()
        {
            Staffs = new ObservableCollection<StaffItem>();
            Genders = new ObservableCollection<string> { "Nam", "Nữ", "Khác" };
            Statuses = new ObservableCollection<string> { "Đang làm", "Tạm nghỉ", "Nghỉ việc" };

            AddCommand = new RelayCommand(_ => AddStaff(), _ => CanSaveStaff());
            UpdateCommand = new RelayCommand(_ => UpdateStaff(), _ => SelectedStaff != null && CanSaveStaff());
            DeleteCommand = new RelayCommand(_ => DeleteStaff(), _ => SelectedStaff != null);
            ClearCommand = new RelayCommand(_ => ClearForm());

            LoadData();
            ClearForm();
        }

        public ObservableCollection<StaffItem> Staffs
        {
            get { return _staffs; }
            set
            {
                _staffs = value;
                OnPropertyChanged(nameof(Staffs));
                RefreshSummary();
            }
        }

        public ObservableCollection<VaiTro> Roles
        {
            get { return _roles; }
            set
            {
                _roles = value;
                OnPropertyChanged(nameof(Roles));
            }
        }

        public ObservableCollection<string> Genders { get; private set; }

        public ObservableCollection<string> Statuses { get; private set; }

        public StaffItem SelectedStaff
        {
            get { return _selectedStaff; }
            set
            {
                _selectedStaff = value;
                OnPropertyChanged(nameof(SelectedStaff));

                if (_selectedStaff != null)
                {
                    EmployeeId = _selectedStaff.EmployeeId;
                    FullName = _selectedStaff.FullName;
                    Gender = _selectedStaff.Gender;
                    BirthDate = _selectedStaff.BirthDate;
                    Phone = _selectedStaff.Phone;
                    Email = _selectedStaff.Email;
                    Address = _selectedStaff.Address;
                    Username = _selectedStaff.Username;
                    Password = _selectedStaff.Password;
                    SelectedRole = Roles.FirstOrDefault(x => x.MaVaiTro == _selectedStaff.RoleId);
                    StartDate = _selectedStaff.StartDate;
                    SalaryText = _selectedStaff.BaseSalary.ToString("0");
                    Status = _selectedStaff.Status;
                }

                RefreshCommands();
            }
        }

        public string EmployeeId
        {
            get { return _employeeId; }
            set
            {
                _employeeId = value;
                OnPropertyChanged(nameof(EmployeeId));
            }
        }

        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
                RefreshCommands();
            }
        }

        public string Gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
                OnPropertyChanged(nameof(Gender));
                RefreshCommands();
            }
        }

        public DateTime? BirthDate
        {
            get { return _birthDate; }
            set
            {
                _birthDate = value;
                OnPropertyChanged(nameof(BirthDate));
                OnPropertyChanged(nameof(StartDate));
                RefreshCommands();
            }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
                RefreshCommands();
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                RefreshCommands();
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
                RefreshCommands();
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                RefreshCommands();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                RefreshCommands();
            }
        }

        public VaiTro SelectedRole
        {
            get { return _selectedRole; }
            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
                RefreshCommands();
            }
        }

        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(BirthDate));
                RefreshCommands();
            }
        }

        public string SalaryText
        {
            get { return _salaryText; }
            set
            {
                _salaryText = value;
                OnPropertyChanged(nameof(SalaryText));
                RefreshCommands();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                RefreshCommands();
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplySearch();
            }
        }

        public string StaffCountText
        {
            get { return Staffs != null ? Staffs.Count.ToString() : "0"; }
        }

        public string ActiveStaffCountText
        {
            get { return Staffs != null ? Staffs.Count(x => x.Status == "Đang làm").ToString() : "0"; }
        }

        public string OnLeaveStaffCountText
        {
            get { return Staffs != null ? Staffs.Count(x => x.Status != "Đang làm").ToString() : "0"; }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        public string Error
        {
            get { return string.Empty; }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(BirthDate))
                {
                    return ValidateBirthDate();
                }

                if (columnName == nameof(StartDate))
                {
                    return ValidateStartDate();
                }

                return string.Empty;
            }
        }

        private void LoadData()
        {
            try
            {
                Roles = new ObservableCollection<VaiTro>(db.VaiTroes.OrderBy(x => x.MaVaiTro).ToList());

                _allStaffs = new ObservableCollection<StaffItem>(
                    db.NhanViens
                      .Include(x => x.VaiTro)
                      .OrderBy(x => x.MaNhanVien)
                      .ToList()
                      .Select(x => StaffItem.FromEntity(x)));

                ApplySearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách nhân viên: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySearch()
        {
            string keyword = (SearchText ?? string.Empty).Trim().ToLower();
            var query = _allStaffs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    SafeLower(x.EmployeeId).Contains(keyword) ||
                    SafeLower(x.FullName).Contains(keyword) ||
                    SafeLower(x.Phone).Contains(keyword) ||
                    SafeLower(x.Username).Contains(keyword) ||
                    SafeLower(x.RoleName).Contains(keyword) ||
                    SafeLower(x.Status).Contains(keyword));
            }

            Staffs = new ObservableCollection<StaffItem>(query);
        }

        private void AddStaff()
        {
            string validationMessage;
            decimal salary;

            if (!ValidateStaffForm(out validationMessage, out salary))
            {
                MessageBox.Show(validationMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string newId = GenerateNewEmployeeId();
                string normalizedUsername = Username.Trim();

                if (db.NhanViens.Any(x => x.TenDangNhap == normalizedUsername))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var staff = new NhanVien
                {
                    MaNhanVien = newId,
                    HoTen = FullName.Trim(),
                    GioiTinh = NormalizeText(Gender, 10),
                    NgaySinh = BirthDate,
                    SoDienThoai = NormalizeText(Phone, 15),
                    Email = NormalizeText(Email, 100),
                    DiaChi = NormalizeText(Address, 200),
                    AnhDaiDien = DefaultAvatarPath,
                    TenDangNhap = normalizedUsername,
                    MatKhau = Password.Trim(),
                    MaVaiTro = SelectedRole.MaVaiTro,
                    NgayVaoLam = StartDate,
                    LuongCoBan = salary,
                    TrangThai = string.IsNullOrWhiteSpace(Status) ? "Đang làm" : Status
                };

                db.NhanViens.Add(staff);
                db.SaveChanges();

                LoadData();
                ClearForm();

                MessageBox.Show("Đã thêm nhân viên.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể thêm nhân viên: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStaff()
        {
            if (SelectedStaff == null)
            {
                MessageBox.Show("Bạn chưa chọn nhân viên cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string validationMessage;
            decimal salary;

            if (!ValidateStaffForm(out validationMessage, out salary))
            {
                MessageBox.Show(validationMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string id = SelectedStaff.EmployeeId;
                string normalizedUsername = Username.Trim();
                var staff = db.NhanViens.FirstOrDefault(x => x.MaNhanVien == id);

                if (staff == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (db.NhanViens.Any(x => x.MaNhanVien != id && x.TenDangNhap == normalizedUsername))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                staff.HoTen = FullName.Trim();
                staff.GioiTinh = NormalizeText(Gender, 10);
                staff.NgaySinh = BirthDate;
                staff.SoDienThoai = NormalizeText(Phone, 15);
                staff.Email = NormalizeText(Email, 100);
                staff.DiaChi = NormalizeText(Address, 200);
                staff.TenDangNhap = normalizedUsername;
                staff.MatKhau = Password.Trim();
                staff.MaVaiTro = SelectedRole.MaVaiTro;
                staff.NgayVaoLam = StartDate;
                staff.LuongCoBan = salary;
                staff.TrangThai = string.IsNullOrWhiteSpace(Status) ? "Đang làm" : Status;

                if (string.IsNullOrWhiteSpace(staff.AnhDaiDien))
                {
                    staff.AnhDaiDien = DefaultAvatarPath;
                }

                db.SaveChanges();

                LoadData();
                SelectedStaff = Staffs.FirstOrDefault(x => x.EmployeeId == id);

                MessageBox.Show("Đã sửa nhân viên.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể sửa nhân viên: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteStaff()
        {
            if (SelectedStaff == null)
            {
                MessageBox.Show("Bạn chưa chọn nhân viên cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string id = SelectedStaff.EmployeeId;
                var staff = db.NhanViens.FirstOrDefault(x => x.MaNhanVien == id);

                if (staff == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool usedInInvoice = db.HoaDons.Any(x => x.MaNhanVien == id);
                bool usedInImportReceipt = db.PhieuNhaps.Any(x => x.MaNhanVien == id);

                if (usedInInvoice || usedInImportReceipt)
                {
                    var result = MessageBox.Show(
                        "Nhân viên này đã phát sinh hóa đơn hoặc phiếu nhập nên không thể xóa cứng. Bạn có muốn chuyển sang trạng thái Nghỉ việc không?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    staff.TrangThai = "Nghỉ việc";
                    db.SaveChanges();
                }
                else
                {
                    var result = MessageBox.Show("Bạn chắc chắn muốn xóa nhân viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    db.NhanViens.Remove(staff);
                    db.SaveChanges();
                }

                LoadData();
                ClearForm();

                MessageBox.Show("Đã xử lý nhân viên.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa nhân viên: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            _selectedStaff = null;
            OnPropertyChanged(nameof(SelectedStaff));

            EmployeeId = GenerateNewEmployeeId();
            FullName = string.Empty;
            Gender = Genders.FirstOrDefault();
            BirthDate = null;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            SelectedRole = Roles != null ? Roles.FirstOrDefault(x => x.MaVaiTro == "VT03") ?? Roles.FirstOrDefault() : null;
            StartDate = DateTime.Today;
            SalaryText = "0";
            Status = "Đang làm";
            RefreshCommands();
        }

        private bool CanSaveStaff()
        {
            decimal salary;
            return !string.IsNullOrWhiteSpace(FullName)
                   && !string.IsNullOrWhiteSpace(Username)
                   && !string.IsNullOrWhiteSpace(Password)
                   && SelectedRole != null
                   && TryParseSalary(out salary)
                   && !HasDateValidationError();
        }

        private bool ValidateStaffForm(out string message, out decimal salary)
        {
            salary = 0;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                message = "Bạn chưa nhập họ tên nhân viên.";
                return false;
            }

            if (FullName.Trim().Length > 100)
            {
                message = "Họ tên không được quá 100 ký tự.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                message = "Bạn chưa nhập tên đăng nhập.";
                return false;
            }

            if (Username.Trim().Length > 50)
            {
                message = "Tên đăng nhập không được quá 50 ký tự.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                message = "Bạn chưa nhập mật khẩu.";
                return false;
            }

            if (Password.Trim().Length > 100)
            {
                message = "Mật khẩu không được quá 100 ký tự.";
                return false;
            }

            if (SelectedRole == null)
            {
                message = "Bạn chưa chọn vai trò.";
                return false;
            }

            string dateValidationMessage = ValidateBirthDate();
            if (!string.IsNullOrWhiteSpace(dateValidationMessage))
            {
                message = dateValidationMessage;
                return false;
            }

            dateValidationMessage = ValidateStartDate();
            if (!string.IsNullOrWhiteSpace(dateValidationMessage))
            {
                message = dateValidationMessage;
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

            if (!TryParseSalary(out salary))
            {
                message = "Lương cơ bản phải là số không âm.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private bool HasDateValidationError()
        {
            return !string.IsNullOrWhiteSpace(ValidateBirthDate())
                   || !string.IsNullOrWhiteSpace(ValidateStartDate());
        }

        private string ValidateBirthDate()
        {
            if (!BirthDate.HasValue)
            {
                return string.Empty;
            }

            DateTime birthDate = BirthDate.Value.Date;

            if (birthDate >= DateTime.Today)
            {
                return "Ngày sinh phải nhỏ hơn ngày hiện tại.";
            }

            if (StartDate.HasValue && birthDate > StartDate.Value.Date)
            {
                return "Ngày sinh không được lớn hơn ngày vào làm.";
            }

            return string.Empty;
        }

        private string ValidateStartDate()
        {
            if (!StartDate.HasValue)
            {
                return "Bạn chưa chọn ngày vào làm.";
            }

            DateTime startDate = StartDate.Value.Date;

            if (startDate > DateTime.Today)
            {
                return "Ngày vào làm không được lớn hơn ngày hiện tại.";
            }

            if (BirthDate.HasValue && startDate < BirthDate.Value.Date)
            {
                return "Ngày vào làm không được nhỏ hơn ngày sinh.";
            }

            return string.Empty;
        }

        private bool TryParseSalary(out decimal salary)
        {
            string value = (SalaryText ?? string.Empty).Trim().Replace(",", string.Empty).Replace(".", string.Empty);
            return decimal.TryParse(value, out salary) && salary >= 0;
        }

        private string GenerateNewEmployeeId()
        {
            int max = 0;
            foreach (var id in db.NhanViens.Select(x => x.MaNhanVien).ToList())
            {
                int number = ExtractNumber(id);
                if (number > max)
                {
                    max = number;
                }
            }

            return "NV" + (max + 1).ToString("D2");
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

        private static int ExtractNumber(string value)
        {
            string digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            int number;
            return int.TryParse(digits, out number) ? number : 0;
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
            OnPropertyChanged(nameof(StaffCountText));
            OnPropertyChanged(nameof(ActiveStaffCountText));
            OnPropertyChanged(nameof(OnLeaveStaffCountText));
        }

        private void RefreshCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class StaffItem
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal BaseSalary { get; set; }
        public string Status { get; set; }

        public static StaffItem FromEntity(NhanVien staff)
        {
            return new StaffItem
            {
                EmployeeId = staff.MaNhanVien,
                FullName = staff.HoTen,
                Gender = staff.GioiTinh,
                BirthDate = staff.NgaySinh,
                Phone = staff.SoDienThoai,
                Email = staff.Email,
                Address = staff.DiaChi,
                Username = staff.TenDangNhap,
                Password = staff.MatKhau,
                RoleId = staff.MaVaiTro,
                RoleName = staff.VaiTro != null ? staff.VaiTro.TenVaiTro : staff.MaVaiTro,
                StartDate = staff.NgayVaoLam,
                BaseSalary = staff.LuongCoBan,
                Status = staff.TrangThai
            };
        }
    }
}
