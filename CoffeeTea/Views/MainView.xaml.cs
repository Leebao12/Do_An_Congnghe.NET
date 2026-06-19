using System.Windows;
using CoffeeTea.Models;
using CoffeeTea.ViewModels;

namespace CoffeeTea.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView() : this(null)
        {
        }

        public MainView(NhanVien authenticatedUser)
        {
            InitializeComponent();
            DataContext = new MainViewModel(authenticatedUser, HandleLogout);
        }

        private void HandleLogout()
        {
            MessageBoxResult result = MessageBox.Show(
            "Bạn có chắc chắn muốn đăng xuất không?",
            "Xác nhận đăng xuất",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            LoginView loginView = new LoginView();
            Application.Current.MainWindow = loginView;
            loginView.Show();
            Close();
        }
    }
}
