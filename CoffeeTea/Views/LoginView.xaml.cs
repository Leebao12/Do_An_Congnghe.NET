using System.Windows;
using CoffeeTea.Models;
using CoffeeTea.ViewModels;

namespace CoffeeTea.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel(HandleLoginSucceeded);
        }

        private void HandleLoginSucceeded(NhanVien authenticatedUser)
        {
            MainView mainView = new MainView(authenticatedUser);
            Application.Current.MainWindow = mainView;
            mainView.Show();
            Close();
        }
    }
}
