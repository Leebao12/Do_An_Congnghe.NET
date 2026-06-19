using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoffeeTea.Models;

namespace CoffeeTea.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly Action<NhanVien> _loginSucceededAction;
        private string _username;
        private string _errorMessage;
        private bool _isAuthenticating;

        public LoginViewModel(Action<NhanVien> loginSucceededAction)
        {
            _loginSucceededAction = loginSucceededAction;
            LoginCommand = new RelayCommand(ExecuteLogin, _ => !IsAuthenticating);
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
            QuickLoginCommand = new RelayCommand(ExecuteQuickLogin, _ => !IsAuthenticating);
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

        public bool IsAuthenticating
        {
            get { return _isAuthenticating; }
            set
            {
                if (_isAuthenticating == value)
                {
                    return;
                }

                _isAuthenticating = value;
                OnPropertyChanged(nameof(IsAuthenticating));
                RaiseCommandStates();
            }
        }

        public ICommand LoginCommand { get; private set; }

        public ICommand ExitCommand { get; private set; }

        public ICommand QuickLoginCommand { get; private set; }

        private void ExecuteLogin(object parameter)
        {
            PasswordBox passwordBox = parameter as PasswordBox;
            string password = passwordBox != null ? passwordBox.Password : string.Empty;
            string username = Username != null ? Username.Trim() : string.Empty;

            Authenticate(username, password);
        }
        // Đăng nhập nhanh cho các tài khoản mẫu
        private void ExecuteQuickLogin(object parameter)
        {
            string userName = parameter != null ? parameter.ToString() : string.Empty;

            if (string.IsNullOrWhiteSpace(userName))
            {
                return;
            }

            string password;
            switch (userName.Trim())
            {
                case "admin":
                    Username = "admin";
                    password = "123456";
                    break;

                case "quanly1":
                    Username = "quanly1";
                    password = "123456";
                    break;

                case "nhanvien1":
                    Username = "nhanvien1";
                    password = "123456";
                    break;

                default:
                    ErrorMessage = "Tài khoản đăng nhập nhanh không hợp lệ.";
                    return;
            }

            Authenticate(Username, password);
        }

        private void Authenticate(string username, string password)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return;
            }

            try
            {
                IsAuthenticating = true;

                using (QL_CoffeeTeaEntities context = new QL_CoffeeTeaEntities())
                {
                    NhanVien user = context.NhanViens
                        .Include(nv => nv.VaiTro)
                        .FirstOrDefault(nv => nv.TenDangNhap == username && nv.MatKhau == password);

                    if (user == null)
                    {
                        ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                        return;
                    }

                    if (!IsAccountActive(user.TrangThai))
                    {
                        ErrorMessage = "Tài khoản đã bị khóa hoặc không còn hoạt động.";
                        return;
                    }

                    _loginSucceededAction?.Invoke(user);
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể kết nối dữ liệu. Vui lòng kiểm tra lại SQL Server.";
            }
            finally
            {
                IsAuthenticating = false;
            }
        }

        private static bool IsAccountActive(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            string normalized = status.Trim().ToLowerInvariant();
            return normalized == "dang lam"
                || normalized == "đang làm"
                || normalized == "dang lam viec"
                || normalized == "đang làm việc";
        }

        private void RaiseCommandStates()
        {
            RelayCommand loginCommand = LoginCommand as RelayCommand;
            if (loginCommand != null)
            {
                loginCommand.RaiseCanExecuteChanged();
            }

            RelayCommand quickLoginCommand = QuickLoginCommand as RelayCommand;
            if (quickLoginCommand != null)
            {
                quickLoginCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
