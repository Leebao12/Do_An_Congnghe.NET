using System.Windows.Controls;
using CoffeeTea.Models;
using CoffeeTea.ViewModels;

namespace CoffeeTea.Views
{
    /// <summary>
    /// Interaction logic for UCSettings.xaml
    /// </summary>
    public partial class UCSettings : UserControl
    {
        public UCSettings() : this(null)
        {
        }

        public UCSettings(NhanVien authenticatedUser)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(authenticatedUser);
        }
    }
}
