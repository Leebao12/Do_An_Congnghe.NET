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
    public class InvoiceDetailModel
    {
        public string MaHoaDon { get; set; }
        public string TenBan { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public ObservableCollection<CartItemModel> Items { get; set; }
        public string MaBan { get; set; }     
        public string MaNhanVien { get; set; }
        public string PhuongThucTT { get; set; }
        public string TrangThai { get; set; }
    }
    public class PaymentViewModel : BaseViewModel
    {
        private readonly Action _goBackAction;
        private bool _isPaid;
        public InvoiceDetailModel InvoiceDetails { get; set; }
        public ObservableCollection<string> PaymentMethods { get; set; }

        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set { _selectedPaymentMethod = value; OnPropertyChanged(nameof(SelectedPaymentMethod)); }
        }

        private string _customerGivenAmount = "0";
        public string CustomerGivenAmount
        {
            get => _customerGivenAmount;
            set
            {
                _customerGivenAmount = value;
                OnPropertyChanged(nameof(CustomerGivenAmount));
                OnPropertyChanged(nameof(ChangeAmount));
            }
        }
        public decimal ChangeAmount
        {
            get
            {
                if (decimal.TryParse(CustomerGivenAmount, out decimal given))
                    return given - InvoiceDetails.TongTien > 0 ? given - InvoiceDetails.TongTien : 0;
                return 0;
            }
        }
        public bool IsPaid
        {
            get => _isPaid;
            set
            {
                if (_isPaid == value) return;

                _isPaid = value;
                OnPropertyChanged(nameof(IsPaid));
                OnPropertyChanged(nameof(CanEditPayment));
                OnPropertyChanged(nameof(PaymentStatusText));
                (ConfirmPaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool CanEditPayment => !IsPaid;

        public string PaymentStatusText => IsPaid ? "Đã thanh toán" : "Chờ thanh toán";

        public ICommand CancelCommand { get; }
        public ICommand ConfirmPaymentCommand { get; }

        public PaymentViewModel(InvoiceDetailModel invoice, Action goBackAction = null)
        {
            InvoiceDetails = invoice;
            _goBackAction = goBackAction;

            PaymentMethods = new ObservableCollection<string> { "Tiền mặt", "Chuyển khoản", "Thẻ tín dụng" };
            SelectedPaymentMethod = !string.IsNullOrWhiteSpace(invoice.PhuongThucTT) ? invoice.PhuongThucTT : "Tiền mặt";
            IsPaid = string.Equals(invoice.TrangThai, "Đã thanh toán", StringComparison.OrdinalIgnoreCase);

            CancelCommand = new RelayCommand(_ => CancelPayment());
            ConfirmPaymentCommand = new RelayCommand(_ => ConfirmPayment(), _ => !IsPaid);
        }

        private void ConfirmPayment()
        {
            if (IsPaid) return;

            if (!decimal.TryParse(CustomerGivenAmount, out decimal givenAmount))
            {
                MessageBox.Show("Thanh toán không thành công! Vui lòng nhập số tiền hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (givenAmount < InvoiceDetails.TongTien)
            {
                MessageBox.Show("Thanh toán không thành công! Số tiền khách đưa không đủ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using (var context = new QL_CoffeeTeaEntities())
                {
                    string maHD = InvoiceDetails.MaHoaDon;
                    if (string.IsNullOrEmpty(maHD))
                    {
                        MessageBox.Show("Không có hóa đơn đang phục vụ để thanh toán.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    HoaDon hoaDon = context.HoaDons.FirstOrDefault(h => h.MaHoaDon == maHD);
                    if (hoaDon == null)
                    {
                        MessageBox.Show("Không tìm thấy hóa đơn trong CSDL.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    hoaDon.MaBan = InvoiceDetails.MaBan;
                    hoaDon.MaNhanVien = InvoiceDetails.MaNhanVien;
                    hoaDon.TongTien = InvoiceDetails.TongTien;
                    hoaDon.PhuongThucTT = SelectedPaymentMethod;
                    hoaDon.TrangThai = "Đã thanh toán";
                    InvoiceDetails.MaHoaDon = maHD;
                    InvoiceDetails.PhuongThucTT = SelectedPaymentMethod;
                    InvoiceDetails.TrangThai = "Đã thanh toán";

                    if (!string.IsNullOrEmpty(InvoiceDetails.MaHoaDon))
                    {
                        foreach (var detail in context.ChiTietHoaDons.Where(ct => ct.MaHoaDon == maHD))
                        {
                            detail.ThanhTien = detail.SoLuong * detail.DonGia;
                        }
                    }

                    var ban = context.Bans.FirstOrDefault(b => b.MaBan == InvoiceDetails.MaBan);
                    if (ban != null)
                    {
                        ban.TrangThai = "Trống";
                    }

        
                    context.SaveChanges();
                }

                MessageBox.Show($"Thanh toán thành công {InvoiceDetails.TongTien:N0} VNĐ qua {SelectedPaymentMethod}!",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                IsPaid = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CancelPayment()
        {
            _goBackAction?.Invoke();
        }
    }
}
