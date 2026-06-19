using CoffeeTea.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoffeeTea.ViewModels
{
    public class TableStatusViewModel : BaseViewModel
    {
        private readonly Action<Ban> _openOrderAction;
        private readonly Action<Ban> _openPaymentAction;
        private QL_CoffeeTeaEntities _context = new QL_CoffeeTeaEntities();
        public ObservableCollection<string> Areas { get; set; }
        public ObservableCollection<string> StatusFilters { get; set; }

        private ObservableCollection<Ban> _tableList;
        public ObservableCollection<Ban> TableList
        {
            get => _tableList;
            set { _tableList = value; OnPropertyChanged(nameof(TableList)); }
        }

        private string _selectedArea;
        public string SelectedArea
        {
            get => _selectedArea;
            set { _selectedArea = value; OnPropertyChanged(nameof(SelectedArea)); FilterTables(); }
        }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); FilterTables(); }
        }

        public ICommand TableClickCommand { get; }

        public TableStatusViewModel(Action<Ban> openOrderAction = null, Action<Ban> openPaymentAction = null)
        {
            _openOrderAction = openOrderAction;
            _openPaymentAction = openPaymentAction;
            LoadData();
            TableClickCommand = new RelayCommand(param => HandleTableClick(param as Ban));
        }

        private void LoadData()
        {
            var areas = _context.Bans.Select(b => b.KhuVuc).Distinct().ToList();
            areas.Insert(0, "Tất cả khu vực");
            Areas = new ObservableCollection<string>(areas);

            var statuses = _context.Bans.Select(b => b.TrangThai).Distinct().ToList();
            statuses.Insert(0, "Tất cả trạng thái");
            StatusFilters = new ObservableCollection<string>(statuses);
            OnPropertyChanged(nameof(Areas));
            OnPropertyChanged(nameof(StatusFilters));

            SelectedArea = "Tất cả khu vực";
            SelectedStatus = "Tất cả trạng thái";
        }

        private void FilterTables()
        {
            var query = _context.Bans.AsQueryable();

            if (!string.IsNullOrEmpty(SelectedArea) && SelectedArea != "Tất cả khu vực")
            {
                query = query.Where(b => b.KhuVuc == SelectedArea);
            }

            if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != "Tất cả trạng thái")
            {
                query = query.Where(b => b.TrangThai == SelectedStatus);
            }

            TableList = new ObservableCollection<Ban>(query.ToList());
        }

        private void HandleTableClick(Ban table)
        {
            if (table == null) return;

            string status = table.TrangThai != null ? table.TrangThai.Trim() : string.Empty;
            if (string.Equals(status, "Trống", StringComparison.OrdinalIgnoreCase))
            {
                _openOrderAction?.Invoke(table);
                return;
            }

            if (string.Equals(status, "Đang phục vụ", StringComparison.OrdinalIgnoreCase))
            {
                _openPaymentAction?.Invoke(table);
                return;
            }

            if (string.Equals(status, "Bảo trì", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"{table.TenBan} đang bảo trì, không thể lập hóa đơn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.Equals(status, "Đã đặt", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"{table.TenBan} đã được đặt trước, không thể lập hóa đơn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show($"{table.TenBan} hiện có trạng thái \"{table.TrangThai}\" nên chưa thể xử lý.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
