using System.Windows.Controls;
using CoffeeTea.Models;
using CoffeeTea.ViewModels;

namespace CoffeeTea.Views
{
    /// <summary>
    /// Interaction logic for UCDashboardView.xaml
    /// </summary>
    public partial class UCDashboardView : UserControl
    {
        public UCDashboardView() : this(null)
        {
        }

        public UCDashboardView(NhanVien authenticatedUser)
        {
            InitializeComponent();
            DataContext = new DashboardViewModel(authenticatedUser);
        }
    }
}
