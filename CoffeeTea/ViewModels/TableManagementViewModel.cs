using CoffeeTea.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CoffeeTea.ViewModels
{
    public class TableManagementViewModel : BaseViewModel
    {
        private const string ServingStatus = "Đang phục vụ";
        private readonly QL_CoffeeTeaEntities db = new QL_CoffeeTeaEntities();
        private List<Ban> _allTables = new List<Ban>();

        private ObservableCollection<Ban> _tables;
        private Ban _selectedTable;
        private string _tableId;
        private string _tableName;
        private string _area;
        private string _seatCountText;
        private string _status;
        private string _searchText;
        private string _selectedStatusFilter;

        public TableManagementViewModel()
        {
            Statuses = new ObservableCollection<string> { "Trống", "Đang phục vụ", "Đã đặt", "Bảo trì" };
            StatusFilters = new ObservableCollection<string> { "Tất cả trạng thái", "Trống", "Đang phục vụ", "Đã đặt", "Bảo trì" };

            AddCommand = new RelayCommand(_ => AddTable(), _ => CanSaveTable());
            UpdateCommand = new RelayCommand(_ => UpdateTable(), _ => SelectedTable != null && CanSaveTable());
            DeleteCommand = new RelayCommand(_ => DeleteTable(), _ => SelectedTable != null);
            ClearCommand = new RelayCommand(_ => ClearForm());

            LoadData();
            ClearForm();
        }

        public ObservableCollection<Ban> Tables
        {
            get { return _tables; }
            set
            {
                _tables = value;
                OnPropertyChanged(nameof(Tables));
                RefreshSummary();
            }
        }

        public ObservableCollection<string> Statuses { get; private set; }

        public ObservableCollection<string> StatusFilters { get; private set; }

        public Ban SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                _selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));

                if (_selectedTable != null)
                {
                    TableId = _selectedTable.MaBan;
                    TableName = _selectedTable.TenBan;
                    Area = _selectedTable.KhuVuc;
                    SeatCountText = _selectedTable.SoChoNgoi.ToString();
                    Status = _selectedTable.TrangThai;
                }

                RefreshCommands();
            }
        }

        public string TableId
        {
            get { return _tableId; }
            set
            {
                _tableId = value;
                OnPropertyChanged(nameof(TableId));
            }
        }

        public string TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                OnPropertyChanged(nameof(TableName));
                RefreshCommands();
            }
        }

        public string Area
        {
            get { return _area; }
            set
            {
                _area = value;
                OnPropertyChanged(nameof(Area));
                RefreshCommands();
            }
        }

        public string SeatCountText
        {
            get { return _seatCountText; }
            set
            {
                _seatCountText = value;
                OnPropertyChanged(nameof(SeatCountText));
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
                ApplyFilter();
            }
        }

        public string SelectedStatusFilter
        {
            get { return _selectedStatusFilter; }
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged(nameof(SelectedStatusFilter));
                ApplyFilter();
            }
        }

        public string TableCountText
        {
            get { return Tables != null ? Tables.Count.ToString() : "0"; }
        }

        public string EmptyTableCountText
        {
            get { return Tables != null ? Tables.Count(x => x.TrangThai == "Trống").ToString() : "0"; }
        }

        public string ServingTableCountText
        {
            get { return Tables != null ? Tables.Count(x => x.TrangThai == "Đang phục vụ").ToString() : "0"; }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        private void LoadData()
        {
            try
            {
                _allTables = db.Bans.OrderBy(x => x.MaBan).ToList();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách bàn: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            var query = _allTables.AsEnumerable();
            string keyword = (SearchText ?? string.Empty).Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    SafeLower(x.MaBan).Contains(keyword) ||
                    SafeLower(x.TenBan).Contains(keyword) ||
                    SafeLower(x.KhuVuc).Contains(keyword) ||
                    SafeLower(x.TrangThai).Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(SelectedStatusFilter) && SelectedStatusFilter != "Tất cả trạng thái")
            {
                query = query.Where(x => x.TrangThai == SelectedStatusFilter);
            }

            Tables = new ObservableCollection<Ban>(query.ToList());
        }

        private void AddTable()
        {
            string validationMessage;
            int seatCount;

            if (!ValidateTableForm(out validationMessage, out seatCount))
            {
                MessageBox.Show(validationMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string newId = GenerateNewTableId();
                string normalizedName = TableName.Trim();

                if (db.Bans.Any(x => x.TenBan == normalizedName))
                {
                    MessageBox.Show("Tên bàn đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var table = new Ban
                {
                    MaBan = newId,
                    TenBan = normalizedName,
                    KhuVuc = NormalizeText(Area, 50),
                    SoChoNgoi = seatCount,
                    TrangThai = string.IsNullOrWhiteSpace(Status) ? "Trống" : Status
                };

                db.Bans.Add(table);
                db.SaveChanges();

                LoadData();
                ClearForm();

                MessageBox.Show("Đã thêm bàn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể thêm bàn: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTable()
        {
            if (SelectedTable == null)
            {
                MessageBox.Show("Bạn chưa chọn bàn cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string validationMessage;
            int seatCount;

            if (!ValidateTableForm(out validationMessage, out seatCount))
            {
                MessageBox.Show(validationMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string id = SelectedTable.MaBan;
                string normalizedName = TableName.Trim();
                var table = db.Bans.FirstOrDefault(x => x.MaBan == id);

                if (table == null)
                {
                    MessageBox.Show("Không tìm thấy bàn cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (IsServing(table.TrangThai))
                {
                    MessageBox.Show("Bàn đang phục vụ nên không thể sửa thông tin. Vui lòng thanh toán hoặc chuyển bàn về trạng thái phù hợp trước.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoadData();
                    SelectedTable = Tables.FirstOrDefault(x => x.MaBan == id);
                    return;
                }

                if (db.Bans.Any(x => x.MaBan != id && x.TenBan == normalizedName))
                {
                    MessageBox.Show("Tên bàn đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                table.TenBan = normalizedName;
                table.KhuVuc = NormalizeText(Area, 50);
                table.SoChoNgoi = seatCount;
                table.TrangThai = string.IsNullOrWhiteSpace(Status) ? "Trống" : Status;

                db.SaveChanges();

                LoadData();
                SelectedTable = Tables.FirstOrDefault(x => x.MaBan == id);

                MessageBox.Show("Đã sửa bàn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể sửa bàn: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTable()
        {
            if (SelectedTable == null)
            {
                MessageBox.Show("Bạn chưa chọn bàn cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string id = SelectedTable.MaBan;
                var table = db.Bans.FirstOrDefault(x => x.MaBan == id);

                if (table == null)
                {
                    MessageBox.Show("Không tìm thấy bàn cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (IsServing(table.TrangThai))
                {
                    MessageBox.Show("Bàn đang phục vụ nên không thể xóa hoặc chuyển trạng thái. Vui lòng thanh toán trước.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoadData();
                    SelectedTable = Tables.FirstOrDefault(x => x.MaBan == id);
                    return;
                }

                bool hasInvoices = db.HoaDons.Any(x => x.MaBan == id);
                if (hasInvoices)
                {
                    var result = MessageBox.Show(
                        "Bàn này đã phát sinh hóa đơn nên không thể xóa cứng. Bạn có muốn chuyển trạng thái sang Bảo trì không?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    table.TrangThai = "Bảo trì";
                    db.SaveChanges();
                }
                else
                {
                    var result = MessageBox.Show("Bạn chắc chắn muốn xóa bàn này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    db.Bans.Remove(table);
                    db.SaveChanges();
                }

                LoadData();
                ClearForm();

                MessageBox.Show("Đã xử lý bàn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa bàn: " + GetInnermostMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            _selectedTable = null;
            OnPropertyChanged(nameof(SelectedTable));

            TableId = GenerateNewTableId();
            TableName = string.Empty;
            Area = "Tầng trệt";
            SeatCountText = "2";
            Status = "Trống";
            RefreshCommands();
        }

        private bool CanSaveTable()
        {
            int seatCount;
            return !string.IsNullOrWhiteSpace(TableName)
                   && !string.IsNullOrWhiteSpace(Area)
                   && TryParseSeatCount(out seatCount);
        }

        private bool ValidateTableForm(out string message, out int seatCount)
        {
            seatCount = 0;

            if (string.IsNullOrWhiteSpace(TableName))
            {
                message = "Bạn chưa nhập tên bàn.";
                return false;
            }

            if (TableName.Trim().Length > 50)
            {
                message = "Tên bàn không được quá 50 ký tự.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Area))
            {
                message = "Bạn chưa nhập khu vực.";
                return false;
            }

            if (Area.Trim().Length > 50)
            {
                message = "Khu vực không được quá 50 ký tự.";
                return false;
            }

            if (!TryParseSeatCount(out seatCount))
            {
                message = "Số chỗ ngồi phải là số nguyên lớn hơn 0.";
                return false;
            }

            if (seatCount > 100)
            {
                message = "Số chỗ ngồi không hợp lệ.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Status))
            {
                message = "Bạn chưa chọn trạng thái.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private bool TryParseSeatCount(out int seatCount)
        {
            return int.TryParse((SeatCountText ?? string.Empty).Trim(), out seatCount) && seatCount > 0;
        }

        private string GenerateNewTableId()
        {
            int max = 0;
            foreach (string id in db.Bans.Select(x => x.MaBan).ToList())
            {
                int number = ExtractNumber(id);
                if (number > max)
                {
                    max = number;
                }
            }

            return "B" + (max + 1).ToString("D2");
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

        private static bool IsServing(string status)
        {
            return string.Equals(status, ServingStatus, StringComparison.OrdinalIgnoreCase);
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
            OnPropertyChanged(nameof(TableCountText));
            OnPropertyChanged(nameof(EmptyTableCountText));
            OnPropertyChanged(nameof(ServingTableCountText));
        }

        private void RefreshCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
