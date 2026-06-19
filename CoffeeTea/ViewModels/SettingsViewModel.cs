using System;
using System.Globalization;
using System.Windows.Input;
using CoffeeTea.Models;
using CoffeeTea.Services;

namespace CoffeeTea.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsStorageService _settingsStorageService;
        private readonly NhanVien _authenticatedUser;

        private string _storeDisplayName;
        private string _storeAddress;
        private string _hotline;
        private string _openTime;
        private string _closeTime;
        private bool _isDarkTheme;
        private string _lastUpdatedInformation;
        private string _successMessage;
        private string _errorMessage;

        public SettingsViewModel(NhanVien authenticatedUser)
        {
            _authenticatedUser = authenticatedUser;
            _settingsStorageService = new SettingsStorageService();

            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
            RestoreDefaultsCommand = new RelayCommand(_ => RestoreDefaults());
            BackupSettingsCommand = new RelayCommand(_ => CreateBackup());

            LoadSettings();
        }

        public ICommand SaveSettingsCommand { get; private set; }

        public ICommand RestoreDefaultsCommand { get; private set; }

        public ICommand BackupSettingsCommand { get; private set; }

        public bool CanEditSystemSettings
        {
            get { return IsSystemSettingsEditor(); }
        }

        public bool CanEditAppearanceSettings
        {
            get { return true; }
        }


        public string RestoreDefaultsButtonText
        {
            get
            {
                return CanEditSystemSettings ? "Khôi phục mặc định" : "Khôi phục giao diện";
            }
        }

        public string StoreDisplayName
        {
            get { return _storeDisplayName; }
            set
            {
                if (_storeDisplayName == value)
                {
                    return;
                }

                _storeDisplayName = value;
                OnPropertyChanged(nameof(StoreDisplayName));
            }
        }

        public string StoreAddress
        {
            get { return _storeAddress; }
            set
            {
                if (_storeAddress == value)
                {
                    return;
                }

                _storeAddress = value;
                OnPropertyChanged(nameof(StoreAddress));
            }
        }

        public string Hotline
        {
            get { return _hotline; }
            set
            {
                if (_hotline == value)
                {
                    return;
                }

                _hotline = value;
                OnPropertyChanged(nameof(Hotline));
            }
        }

        public string OpenTime
        {
            get { return _openTime; }
            set
            {
                if (_openTime == value)
                {
                    return;
                }

                _openTime = value;
                OnPropertyChanged(nameof(OpenTime));
            }
        }

        public string CloseTime
        {
            get { return _closeTime; }
            set
            {
                if (_closeTime == value)
                {
                    return;
                }

                _closeTime = value;
                OnPropertyChanged(nameof(CloseTime));
            }
        }

        public bool IsDarkTheme
        {
            get { return _isDarkTheme; }
            set
            {
                if (_isDarkTheme == value)
                {
                    return;
                }

                _isDarkTheme = value;
                OnPropertyChanged(nameof(IsDarkTheme));
                ThemeManager.ApplyTheme(_isDarkTheme);
            }
        }

        public string LastUpdatedInformation
        {
            get { return _lastUpdatedInformation; }
            set
            {
                if (_lastUpdatedInformation == value)
                {
                    return;
                }

                _lastUpdatedInformation = value;
                OnPropertyChanged(nameof(LastUpdatedInformation));
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

        private void LoadSettings()
        {
            SoftwareSettingsModel settings = _settingsStorageService.Load();
            ApplySettings(settings);
            LastUpdatedInformation = BuildLastUpdatedText(settings);
        }

        private void SaveSettings()
        {
            ClearStatus();

            if (CanEditSystemSettings)
            {
                SaveSystemSettings();
            }
            else
            {
                SaveAppearanceSettings();
            }
        }

        private void SaveSystemSettings()
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                SoftwareSettingsModel settings = BuildCurrentSettings();
                settings.LastUpdatedBy = ResolveUpdatedBy();
                settings.LastUpdatedAt = DateTime.Now;

                _settingsStorageService.Save(settings);

                LastUpdatedInformation = BuildLastUpdatedText(settings);
                SuccessMessage = "Đã lưu cài đặt hệ thống thành công.";
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể lưu cài đặt hệ thống. Vui lòng thử lại.";
            }
        }

        private void SaveAppearanceSettings()
        {
            try
            {
                SoftwareSettingsModel settings = _settingsStorageService.Load();
                settings.IsDarkTheme = IsDarkTheme;
                settings.LastUpdatedBy = ResolveUpdatedBy();
                settings.LastUpdatedAt = DateTime.Now;

                _settingsStorageService.Save(settings);

                LastUpdatedInformation = BuildLastUpdatedText(settings);
                SuccessMessage = "Đã lưu cài đặt giao diện thành công.";
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể lưu cài đặt giao diện. Vui lòng thử lại.";
            }
        }

        private void RestoreDefaults()
        {
            ClearStatus();

            try
            {
                if (CanEditSystemSettings)
                {
                    RestoreSystemDefaults();
                }
                else
                {
                    RestoreAppearanceDefaults();
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể khôi phục cài đặt mặc định.";
            }
        }

        private void RestoreSystemDefaults()
        {
            SoftwareSettingsModel defaults = SoftwareSettingsModel.CreateDefault();
            defaults.LastUpdatedBy = ResolveUpdatedBy();
            defaults.LastUpdatedAt = DateTime.Now;

            ApplySettings(defaults);
            _settingsStorageService.Save(defaults);

            LastUpdatedInformation = BuildLastUpdatedText(defaults);
            SuccessMessage = "Đã khôi phục cài đặt mặc định.";
        }

        private void RestoreAppearanceDefaults()
        {
            SoftwareSettingsModel settings = _settingsStorageService.Load();
            settings.IsDarkTheme = SoftwareSettingsModel.CreateDefault().IsDarkTheme;
            settings.LastUpdatedBy = ResolveUpdatedBy();
            settings.LastUpdatedAt = DateTime.Now;

            ApplySettings(settings);
            _settingsStorageService.Save(settings);

            LastUpdatedInformation = BuildLastUpdatedText(settings);
            SuccessMessage = "Đã khôi phục cài đặt giao diện mặc định.";
        }

        private void CreateBackup()
        {
            ClearStatus();

            if (!CanEditSystemSettings)
            {
                ErrorMessage = "Tài khoản nhân viên không có quyền sao lưu cấu hình hệ thống.";
                return;
            }

            try
            {
                SoftwareSettingsModel settings = BuildCurrentSettings();
                string backupPath = _settingsStorageService.CreateBackup(settings);
                SuccessMessage = string.Format("Đã tạo bản sao lưu cấu hình tại: {0}", backupPath);
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể tạo bản sao lưu cấu hình.";
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(StoreDisplayName))
            {
                ErrorMessage = "Tên hiển thị cửa hàng không được để trống.";
                return false;
            }

            if (!TimeSpan.TryParseExact(OpenTime, "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan openTime))
            {
                ErrorMessage = "Giờ mở cửa không hợp lệ. Định dạng đúng là HH:mm.";
                return false;
            }

            if (!TimeSpan.TryParseExact(CloseTime, "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan closeTime))
            {
                ErrorMessage = "Giờ đóng cửa không hợp lệ. Định dạng đúng là HH:mm.";
                return false;
            }

            if (openTime >= closeTime)
            {
                ErrorMessage = "Giờ mở cửa phải nhỏ hơn giờ đóng cửa.";
                return false;
            }
            return true;
        }

        private SoftwareSettingsModel BuildCurrentSettings()
        {
            return new SoftwareSettingsModel
            {
                StoreDisplayName = Normalize(StoreDisplayName),
                StoreAddress = Normalize(StoreAddress),
                Hotline = Normalize(Hotline),
                OpenTime = Normalize(OpenTime),
                CloseTime = Normalize(CloseTime),
                IsDarkTheme = IsDarkTheme,
                LastUpdatedBy = ResolveUpdatedBy(),
                LastUpdatedAt = DateTime.Now
            };
        }

        private void ApplySettings(SoftwareSettingsModel settings)
        {
            StoreDisplayName = settings.StoreDisplayName;
            StoreAddress = settings.StoreAddress;
            Hotline = settings.Hotline;
            OpenTime = settings.OpenTime;
            CloseTime = settings.CloseTime;
            IsDarkTheme = settings.IsDarkTheme;
        }

        private string ResolveUpdatedBy()
        {
            if (!string.IsNullOrWhiteSpace(_authenticatedUser?.HoTen))
            {
                return _authenticatedUser.HoTen.Trim();
            }

            if (!string.IsNullOrWhiteSpace(_authenticatedUser?.TenDangNhap))
            {
                return _authenticatedUser.TenDangNhap.Trim();
            }

            return "Người dùng hệ thống";
        }

        private static string BuildLastUpdatedText(SoftwareSettingsModel settings)
        {
            return string.Format(
                "Cập nhật lần cuối: {0:dd/MM/yyyy HH:mm} bởi {1}",
                settings.LastUpdatedAt,
                string.IsNullOrWhiteSpace(settings.LastUpdatedBy) ? "Hệ thống" : settings.LastUpdatedBy);
        }

        private void ClearStatus()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private bool IsSystemSettingsEditor()
        {
            string roleCode = _authenticatedUser != null ? _authenticatedUser.MaVaiTro : null;
            string roleName = _authenticatedUser?.VaiTro != null ? _authenticatedUser.VaiTro.TenVaiTro : null;

            if (string.Equals(roleCode, "VT01", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(roleCode, "VT02", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            string normalizedRoleName = roleName.Trim();
            return normalizedRoleName.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalizedRoleName.IndexOf("quản lý", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalizedRoleName.IndexOf("quan ly", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

