using CoffeeTea.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Data.Entity;
using System.Collections.Generic;
using System.Windows;

namespace CoffeeTea.ViewModels
{
    public class DrinkViewModel : BaseViewModel
    {
        private const string ActiveStatus = "Đang bán";
        private const string HiddenStatus = "Ngừng bán";
        private QL_CoffeeTeaEntities db = new QL_CoffeeTeaEntities();

        private ObservableCollection<Mon> _drinks;
        public ObservableCollection<Mon> Drinks
        {
            get { return _drinks; }
            set
            {
                _drinks = value;
                OnPropertyChanged("Drinks");
            }
        }

        private ObservableCollection<DanhMucMon> _categories;
        public ObservableCollection<DanhMucMon> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged("Categories");
            }
        }

        private string _tenMon;
        public string TenMon
        {
            get { return _tenMon; }
            set
            {
                _tenMon = value;
                OnPropertyChanged("TenMon");
            }
        }
        private string _donViTinh;
        public string DonViTinh
        {
            get { return _donViTinh; }
            set { _donViTinh = value; OnPropertyChanged("DonViTinh"); }
        }

        private decimal? _donGia;
        public decimal? DonGia
        {
            get { return _donGia; }
            set
            {
                _donGia = value;
                OnPropertyChanged("DonGia");
            }
        }
        private List<Mon> _allDrinksList;
        private ObservableCollection<DanhMucMon> _filterCategories;
        public ObservableCollection<DanhMucMon> FilterCategories
        {
            get { return _filterCategories; }
            set { _filterCategories = value; OnPropertyChanged("FilterCategories"); }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; OnPropertyChanged("SearchText"); ExecuteFilter(); }
        }

        private DanhMucMon _selectedFilterCategory;
        public DanhMucMon SelectedFilterCategory
        {
            get { return _selectedFilterCategory; }
            set { _selectedFilterCategory = value; OnPropertyChanged("SelectedFilterCategory"); ExecuteFilter(); }
        }

        private DanhMucMon _selectedCategoryInForm;
        public DanhMucMon SelectedCategoryInForm
        {
            get { return _selectedCategoryInForm; }
            set
            {
                _selectedCategoryInForm = value;
                OnPropertyChanged("SelectedCategoryInForm");
            }
        }

        private Mon _selectedDrink;
        public Mon SelectedDrink
        {
            get { return _selectedDrink; }
            set
            {
                _selectedDrink = value;
                OnPropertyChanged("SelectedDrink");
                if (SelectedDrink != null)
                {
                    TenMon = SelectedDrink.TenMon;
                    DonGia = SelectedDrink.DonGia;
                    DonViTinh = SelectedDrink.DonViTinh; 
                    SelectedCategoryInForm = Categories.FirstOrDefault(x => x.MaDanhMuc == SelectedDrink.MaDanhMuc);
                }
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public DrinkViewModel()
        {
            LoadData();

            AddCommand = new RelayCommand(
                 (p) => {
                     var newDrink = new Mon()
                     {
                         MaMon = GenerateNewId(),
                         TenMon = TenMon,
                         DonGia = DonGia ?? 0,
                         MaDanhMuc = SelectedCategoryInForm.MaDanhMuc,
                         DonViTinh = string.IsNullOrEmpty(DonViTinh) ? "Ly" : DonViTinh,
                         TrangThai = ActiveStatus
                      };
                         db.Mons.Add(newDrink);
                         db.SaveChanges();
                         LoadData();
                         ClearInputs();
                         (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
                      },
             (p) => !string.IsNullOrEmpty(TenMon) && !string.IsNullOrEmpty(DonViTinh) && SelectedCategoryInForm != null
             );

            EditCommand = new RelayCommand(
                (p) => {
                    var drink = db.Mons.FirstOrDefault(x => x.MaMon == SelectedDrink.MaMon);
                    if (drink != null)
                    {
                        drink.TenMon = TenMon;
                        drink.DonGia = DonGia ?? 0;
                        drink.MaDanhMuc = SelectedCategoryInForm.MaDanhMuc;
                        drink.DonViTinh = DonViTinh; 

                        db.SaveChanges();
                        LoadData();
                    }
                },
                (p) => SelectedDrink != null
            );

            DeleteCommand = new RelayCommand(
                (p) => {
                    var drink = db.Mons.FirstOrDefault(x => x.MaMon == SelectedDrink.MaMon);
                    if (drink != null)
                    {
                        var result = MessageBox.Show(
                            "Bạn chắc chắn muốn xóa món này?",
                            "Xác nhận",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes)
                        {
                            return;
                        }

                        drink.TrangThai = HiddenStatus;
                        db.SaveChanges();
                        MessageBox.Show("Đã xoá món khỏi danh sách bán hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ClearInputs();
                        (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                        (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    }
                },
                (p) => SelectedDrink != null
            );
        }

        public void LoadData()
        {
            var list = db.Mons
                .Include(m => m.DanhMucMon)
                .Where(m => m.TrangThai == ActiveStatus)
                .ToList();
            _allDrinksList = list;
            Categories = new ObservableCollection<DanhMucMon>(db.DanhMucMons.ToList());
            var tempFilter = new ObservableCollection<DanhMucMon>(Categories);
            tempFilter.Insert(0, new DanhMucMon { TenDanhMuc = "Tất cả", MaDanhMuc = "" });
            FilterCategories = tempFilter;
            ExecuteFilter();
        }

        private void ClearInputs()
        {
            TenMon = string.Empty;
            DonGia = null;
            SelectedCategoryInForm = null;
            SelectedDrink = null;
        }

        private string GenerateNewId()
        {
            var lastDrink = db.Mons.OrderByDescending(x => x.MaMon).FirstOrDefault();
            if (lastDrink == null) return "M01";

            try
            {

                int number = int.Parse(lastDrink.MaMon.Substring(1)) + 1;
                return "M" + number.ToString("D2");
            }
            catch
            {
                return "M" + (db.Mons.Count() + 1).ToString("D2");
            }
        }
        public void ExecuteFilter()
        {
            if (_allDrinksList == null) return;
            var result = _allDrinksList.AsEnumerable();
            if (!string.IsNullOrEmpty(SearchText))
            {
                result = result.Where(x => x.TenMon.ToLower().Contains(SearchText.ToLower()));
            }
            if (SelectedFilterCategory != null && SelectedFilterCategory.TenDanhMuc != "Tất cả")
            {
                result = result.Where(x => x.MaDanhMuc == SelectedFilterCategory.MaDanhMuc);
            }
            Drinks = new ObservableCollection<Mon>(result.ToList());
        }
    }
}
