using CoffeeTea.ViewModels;
using System.Windows.Controls;

namespace CoffeeTea.Views
{
    public partial class UCTableManagement : UserControl
    {
        public UCTableManagement()
        {
            InitializeComponent();
            DataContext = new TableManagementViewModel();
        }
    }
}
