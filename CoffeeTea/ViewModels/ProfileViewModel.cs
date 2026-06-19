using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CoffeeTea.Models;
using CoffeeTea.Services;
using Microsoft.Win32;

namespace CoffeeTea.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly string _maNhanVien;

        private string _fullName;
        private string _username;
        private string _roleName;
        private string _phoneNumber;
        private string _email;
        private string _address;
        private string _anhDaiDien;
        private ImageSource _avatarPath;
        private string _avatarInitials;
        private Visibility _avatarImageVisibility = Visibility.Collapsed;
        private Visibility _avatarInitialsVisibility = Visibility.Visible;

        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;

        private string _successMessage;
        private string _errorMessage;

        public ProfileViewModel(NhanVien authenticatedUser)
        {
            _maNhanVien = authenticatedUser != null ? authenticatedUser.MaNhanVien : null;

            SaveProfileCommand = new RelayCommand(_ => ExecuteSaveProfile(), _ => CanEditProfile);
            ChangePasswordCommand = new RelayCommand(_ => ExecuteChangePassword(), _ => CanEditProfile);
            SelectAvatarCommand = new RelayCommand(_ => ExecuteSelectAvatar(), _ => CanEditProfile);

            LoadProfileData();
        }

        public event Action PasswordInputsClearRequested;

        public ICommand SaveProfileCommand { get; private set; }

        public ICommand ChangePasswordCommand { get; private set; }

        public ICommand SelectAvatarCommand { get; private set; }

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName == value)
                {
                    return;
                }

                _fullName = value;
                OnPropertyChanged(nameof(FullName));
                UpdateAvatarInitials();
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (_username == value)
                {
                    return;
                }

                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string RoleName
        {
            get { return _roleName; }
            set
            {
                if (_roleName == value)
                {
                    return;
                }

                _roleName = value;
                OnPropertyChanged(nameof(RoleName));
            }
        }

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                if (_phoneNumber == value)
                {
                    return;
                }

                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                if (_email == value)
                {
                    return;
                }

                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                if (_address == value)
                {
                    return;
                }

                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string AnhDaiDien
        {
            get { return _anhDaiDien; }
            set
            {
                string normalizedValue = AvatarDisplayService.NormalizeRelativePath(value);
                if (_anhDaiDien == normalizedValue)
                {
                    return;
                }

                _anhDaiDien = normalizedValue;
                OnPropertyChanged(nameof(AnhDaiDien));
                RefreshAvatarDisplay();
            }
        }

        public ImageSource AvatarPath
        {
            get { return _avatarPath; }
            private set
            {
                if (_avatarPath == value)
                {
                    return;
                }

                _avatarPath = value;
                OnPropertyChanged(nameof(AvatarPath));
            }
        }

        public string AvatarInitials
        {
            get { return _avatarInitials; }
            private set
            {
                if (_avatarInitials == value)
                {
                    return;
                }

                _avatarInitials = value;
                OnPropertyChanged(nameof(AvatarInitials));
            }
        }

        public Visibility AvatarImageVisibility
        {
            get { return _avatarImageVisibility; }
            private set
            {
                if (_avatarImageVisibility == value)
                {
                    return;
                }

                _avatarImageVisibility = value;
                OnPropertyChanged(nameof(AvatarImageVisibility));
            }
        }

        public Visibility AvatarInitialsVisibility
        {
            get { return _avatarInitialsVisibility; }
            private set
            {
                if (_avatarInitialsVisibility == value)
                {
                    return;
                }

                _avatarInitialsVisibility = value;
                OnPropertyChanged(nameof(AvatarInitialsVisibility));
            }
        }

        public string CurrentPassword
        {
            get { return _currentPassword; }
            set
            {
                if (_currentPassword == value)
                {
                    return;
                }

                _currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));
            }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                if (_newPassword == value)
                {
                    return;
                }

                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                if (_confirmPassword == value)
                {
                    return;
                }

                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string SuccessMessage
        {
            get { return _successMessage; }
            set
            {
                if (_successMessage == value)
                {
                    return;
                }

                _successMessage = value;
                OnPropertyChanged(nameof(SuccessMessage));
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage == value)
                {
                    return;
                }

                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool CanEditProfile
        {
            get { return !string.IsNullOrWhiteSpace(_maNhanVien); }
        }

        private void LoadProfileData()
        {
            ClearMessages();

            if (!CanEditProfile)
            {
                ErrorMessage = "Không xác định được tài khoản đang đăng nhập.";
                AnhDaiDien = AvatarDisplayService.DefaultAvatarRelativePath;
                return;
            }

            try
            {
                using (QL_CoffeeTeaEntities context = new QL_CoffeeTeaEntities())
                {
                    NhanVien user = context.NhanViens
                        .Include(nv => nv.VaiTro)
                        .FirstOrDefault(nv => nv.MaNhanVien == _maNhanVien);

                    if (user == null)
                    {
                        ErrorMessage = "Không tìm thấy thông tin tài khoản trong cơ sở dữ liệu.";
                        AnhDaiDien = AvatarDisplayService.DefaultAvatarRelativePath;
                        return;
                    }

                    FullName = user.HoTen;
                    Username = user.TenDangNhap;
                    PhoneNumber = user.SoDienThoai;
                    Email = user.Email;
                    Address = user.DiaChi;
                    RoleName = user.VaiTro != null ? user.VaiTro.TenVaiTro : "Không xác định";
                    AnhDaiDien = string.IsNullOrWhiteSpace(user.AnhDaiDien)
                        ? AvatarDisplayService.DefaultAvatarRelativePath
                        : user.AnhDaiDien;
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể tải thông tin tài khoản. Vui lòng kiểm tra kết nối dữ liệu.";
                AnhDaiDien = AvatarDisplayService.DefaultAvatarRelativePath;
            }
        }

        private void ExecuteSaveProfile()
        {
            ClearMessages();

            if (!CanEditProfile)
            {
                ErrorMessage = "Không xác định được tài khoản để cập nhật.";
                return;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Họ tên không được để trống.";
                return;
            }

            try
            {
                using (QL_CoffeeTeaEntities context = new QL_CoffeeTeaEntities())
                {
                    NhanVien user = context.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == _maNhanVien);
                    if (user == null)
                    {
                        ErrorMessage = "Tài khoản không tồn tại hoặc đã bị xóa.";
                        return;
                    }

                    user.HoTen = FullName.Trim();
                    user.SoDienThoai = NormalizeNullable(PhoneNumber);
                    user.Email = NormalizeNullable(Email);
                    user.DiaChi = NormalizeNullable(Address);
                    user.AnhDaiDien = string.IsNullOrWhiteSpace(AnhDaiDien)
                        ? AvatarDisplayService.DefaultAvatarRelativePath
                        : AnhDaiDien;

                    context.SaveChanges();
                }

                SuccessMessage = "Cập nhật hồ sơ thành công.";
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể cập nhật hồ sơ. Vui lòng thử lại.";
            }
        }

        private void ExecuteSelectAvatar()
        {
            ClearMessages();

            if (!CanEditProfile)
            {
                ErrorMessage = "Không xác định được tài khoản để chọn ảnh đại diện.";
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Ảnh đại diện (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Multiselect = false,
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            string sourceFilePath = openFileDialog.FileName;
            string extension = Path.GetExtension(sourceFilePath);
            if (!IsAllowedImageExtension(extension))
            {
                ErrorMessage = "Chỉ được chọn file ảnh .jpg, .jpeg hoặc .png.";
                return;
            }

            try
            {
                string destinationDirectory = AvatarDisplayService.ResolveApplicationPath(AvatarDisplayService.EmployeeAvatarRelativeFolder);
                Directory.CreateDirectory(destinationDirectory);

                string fileName = BuildEmployeeAvatarFileName(extension);
                string relativePath = AvatarDisplayService.EmployeeAvatarRelativeFolder + "/" + fileName;
                string destinationFilePath = AvatarDisplayService.ResolveApplicationPath(relativePath);

                if (!string.Equals(
                    Path.GetFullPath(sourceFilePath),
                    Path.GetFullPath(destinationFilePath),
                    StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(sourceFilePath, destinationFilePath, true);
                }

                AnhDaiDien = relativePath;
                SuccessMessage = "Đã chọn ảnh đại diện. Nhấn Lưu thông tin để lưu vào hồ sơ.";
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể sao chép ảnh đại diện. Vui lòng chọn file ảnh khác.";
            }
        }

        private void ExecuteChangePassword()
        {
            ClearMessages();

            if (!CanEditProfile)
            {
                ErrorMessage = "Không xác định được tài khoản để đổi mật khẩu.";
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ mật khẩu hiện tại, mật khẩu mới và xác nhận mật khẩu.";
                return;
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return;
            }

            if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            {
                ErrorMessage = "Xác nhận mật khẩu không khớp.";
                return;
            }

            if (string.Equals(CurrentPassword, NewPassword, StringComparison.Ordinal))
            {
                ErrorMessage = "Mật khẩu mới phải khác mật khẩu hiện tại.";
                return;
            }

            try
            {
                using (QL_CoffeeTeaEntities context = new QL_CoffeeTeaEntities())
                {
                    NhanVien user = context.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == _maNhanVien);
                    if (user == null)
                    {
                        ErrorMessage = "Tài khoản không tồn tại hoặc đã bị xóa.";
                        return;
                    }

                    if (!string.Equals(user.MatKhau, CurrentPassword, StringComparison.Ordinal))
                    {
                        ErrorMessage = "Mật khẩu hiện tại không chính xác.";
                        return;
                    }

                    user.MatKhau = NewPassword;
                    context.SaveChanges();
                }

                SuccessMessage = "Đổi mật khẩu thành công.";
                ClearPasswordInputs();
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể đổi mật khẩu. Vui lòng thử lại.";
            }
        }

        private void RefreshAvatarDisplay()
        {
            string relativePath = string.IsNullOrWhiteSpace(AnhDaiDien)
                ? AvatarDisplayService.DefaultAvatarRelativePath
                : AnhDaiDien;

            string absolutePath = AvatarDisplayService.ResolveAvatarPath(relativePath);

            ImageSource imageSource = AvatarDisplayService.LoadAvatarImage(absolutePath);
            if (imageSource != null)
            {
                AvatarPath = imageSource;
                AvatarImageVisibility = Visibility.Visible;
                AvatarInitialsVisibility = Visibility.Collapsed;
                return;
            }

            AvatarPath = null;
            AvatarImageVisibility = Visibility.Collapsed;
            AvatarInitialsVisibility = Visibility.Visible;
            UpdateAvatarInitials();
        }

        private void UpdateAvatarInitials()
        {
            AvatarInitials = BuildInitials(FullName);
        }

        private string BuildEmployeeAvatarFileName(string extension)
        {
            string safeEmployeeId = string.Join("_", _maNhanVien.Split(Path.GetInvalidFileNameChars()));
            return safeEmployeeId + extension.ToLowerInvariant();
        }

        private static string BuildInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "?";
            }

            string[] parts = fullName
                .Trim()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return "?";
            }

            if (parts.Length == 1)
            {
                return parts[0].Substring(0, 1).ToUpperInvariant();
            }

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static bool IsAllowedImageExtension(string extension)
        {
            return string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase);
        }

        private void ClearMessages()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
        }

        private void ClearPasswordInputs()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            PasswordInputsClearRequested?.Invoke();
        }

        private static string NormalizeNullable(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
