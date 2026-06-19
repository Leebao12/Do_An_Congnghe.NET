using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CoffeeTea.Models;
using CoffeeTea.Services;

namespace CoffeeTea.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly NhanVien _authenticatedUser;

        public DashboardViewModel(NhanVien authenticatedUser)
        {
            _authenticatedUser = authenticatedUser;

            CultureInfo vietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
            DateTime now = DateTime.Now;
            NhanVien currentAccount = ResolveCurrentAccount();

            string accountName = ResolveAccountName(currentAccount, vietnameseCulture);
            string accountRole = ResolveAccountRole(currentAccount);

            StoreName = "CoffeeTea";
            WelcomeMessage = string.Format("Xin chào, {0}", accountName);
            StoreTagline = "Theo dõi nhanh vận hành cửa hàng, ca làm việc và tài khoản đang sử dụng ứng dụng.";
            OpenHours = "06:30 - 22:30";
            StoreAddress = "Chi nhánh mặc định - cập nhật địa chỉ thực tế khi kết nối dữ liệu cửa hàng.";
            BusinessDate = now.ToString("dddd, dd/MM/yyyy", vietnameseCulture);
            CurrentDateText = now.ToString("dddd, dd/MM/yyyy", vietnameseCulture);
            CurrentShift = ResolveShift(now);
            ShiftText = CurrentShift + " - 07:00 đến 12:00";
            StoreStatus = IsStoreOpen(now) ? "Đang mở cửa" : "Ngoài giờ phục vụ";
            StoreStatusDetail = IsStoreOpen(now)
                ? "Các màn hình nghiệp vụ đã sẵn sàng cho ca làm việc hiện tại."
                : "Hệ thống đang nằm ngoài khung giờ phục vụ mặc định của cửa hàng.";

            CurrentAccountName = accountName;
            CurrentAccountInitial = ResolveInitial(accountName);
            CurrentAccountRole = accountRole;
            CurrentAccountAvatarPath = AvatarDisplayService.LoadAvatarImage(
                AvatarDisplayService.ResolveAvatarPath(currentAccount?.AnhDaiDien));
            CurrentAccountAvatarImageVisibility = CurrentAccountAvatarPath != null ? Visibility.Visible : Visibility.Collapsed;
            CurrentAccountInitialVisibility = CurrentAccountAvatarPath != null ? Visibility.Collapsed : Visibility.Visible;
            SignInSource = ResolveSignInSource(currentAccount);
            DeviceName = Environment.MachineName;
            SessionStartedAt = now.ToString("HH:mm 'ngày' dd/MM/yyyy", vietnameseCulture);

            DashboardScreenCount = "12";
            FunctionalGroupCount = "4";
            ActiveSessionCount = "1";
            ServiceReadiness = "Sẵn sàng";

            LoadDashboardData(now, vietnameseCulture);
        }

        public Geometry RevenueLineGeometry { get; private set; }

        public Geometry RevenueAreaGeometry { get; private set; }

        public ObservableCollection<string> RevenueHourLabels { get; private set; }

        public string WelcomeMessage { get; private set; }

        public string StoreName { get; private set; }

        public string StoreTagline { get; private set; }

        public string OpenHours { get; private set; }

        public string StoreAddress { get; private set; }

        public string BusinessDate { get; private set; }

        public string CurrentDateText { get; private set; }

        public string StoreStatus { get; private set; }

        public string StoreStatusDetail { get; private set; }

        public string CurrentShift { get; private set; }

        public string ShiftText { get; private set; }

        public string CurrentAccountName { get; private set; }

        public string CurrentAccountInitial { get; private set; }

        public ImageSource CurrentAccountAvatarPath { get; private set; }

        public Visibility CurrentAccountAvatarImageVisibility { get; private set; }

        public Visibility CurrentAccountInitialVisibility { get; private set; }

        public string CurrentAccountRole { get; private set; }

        public string SignInSource { get; private set; }

        public string DeviceName { get; private set; }

        public string SessionStartedAt { get; private set; }

        public string DashboardScreenCount { get; private set; }

        public string FunctionalGroupCount { get; private set; }

        public string ActiveSessionCount { get; private set; }

        public string ServiceReadiness { get; private set; }

        public string TodayRevenue { get; private set; }

        public string RevenueMetricTitle { get; private set; }

        public string InvoiceCount { get; private set; }

        public string InvoiceMetricTitle { get; private set; }

        public string ActiveTables { get; private set; }

        public string BestSellerName { get; private set; }

        public string RevenueChartTitle { get; private set; }

        public string RevenueAxisTopLabel { get; private set; }

        public string RevenueAxisHighLabel { get; private set; }

        public string RevenueAxisLowLabel { get; private set; }

        public ObservableCollection<string> FocusItems { get; private set; }

        private void LoadDashboardData(DateTime now, CultureInfo culture)
        {
            DateTime reportDate = now.Date;
            bool isToday = true;

            try
            {
                using (QL_CoffeeTeaEntities context = new QL_CoffeeTeaEntities())
                {
                    DateTime tomorrow = now.Date.AddDays(1);
                    bool hasTodayInvoice = context.HoaDons.Any(hd => hd.NgayLap >= now.Date && hd.NgayLap < tomorrow);

                    if (!hasTodayInvoice)
                    {
                        DateTime? latestInvoiceDate = context.HoaDons
                            .OrderByDescending(hd => hd.NgayLap)
                            .Select(hd => (DateTime?)hd.NgayLap)
                            .FirstOrDefault();

                        if (latestInvoiceDate.HasValue)
                        {
                            reportDate = latestInvoiceDate.Value.Date;
                            isToday = false;
                        }
                    }

                    DateTime reportStart = reportDate.Date;
                    DateTime reportEnd = reportStart.AddDays(1);

                    List<HoaDon> reportInvoices = context.HoaDons
                        .Where(hd => hd.NgayLap >= reportStart && hd.NgayLap < reportEnd)
                        .ToList();

                    List<HoaDon> paidInvoices = reportInvoices
                        .Where(IsPaidInvoice)
                        .ToList();

                    int unpaidInvoiceCount = reportInvoices.Count(hd => !IsPaidInvoice(hd));
                    decimal revenue = paidInvoices.Sum(hd => hd.TongTien);
                    List<Ban> tables = context.Bans.ToList();
                    int activeTableCount = tables.Count(ban => IsActiveTableStatus(ban.TrangThai));
                    int lowStockCount = context.NguyenLieux.Count(nl => nl.SoLuongTon <= nl.MucCanhBao);

                    RevenueMetricTitle = isToday
                        ? "Doanh thu hôm nay"
                        : string.Format("Doanh thu ngày {0}", reportDate.ToString("dd/MM/yyyy", culture));
                    InvoiceMetricTitle = isToday
                        ? "Số hóa đơn hôm nay"
                        : string.Format("Số hóa đơn ngày {0}", reportDate.ToString("dd/MM/yyyy", culture));
                    RevenueChartTitle = isToday
                        ? "Biểu đồ doanh thu hôm nay theo giờ"
                        : string.Format("Biểu đồ doanh thu ngày {0} theo giờ", reportDate.ToString("dd/MM/yyyy", culture));

                    TodayRevenue = FormatCurrency(revenue, culture);
                    InvoiceCount = reportInvoices.Count.ToString("N0", culture);
                    ActiveTables = string.Format("{0} / {1}", activeTableCount.ToString("N0", culture), tables.Count.ToString("N0", culture));
                    BestSellerName = ResolveBestSellerName(context, reportStart, reportEnd);
                    FocusItems = BuildFocusItems(reportDate, culture, reportInvoices.Count, unpaidInvoiceCount, activeTableCount, tables.Count, lowStockCount);

                    LoadRevenueChartData(paidInvoices, reportDate, now, culture);
                }
            }
            catch (Exception ex)
            {
                RevenueMetricTitle = "Doanh thu hôm nay";
                InvoiceMetricTitle = "Số hóa đơn hôm nay";
                RevenueChartTitle = "Biểu đồ doanh thu hôm nay theo giờ";
                TodayRevenue = FormatCurrency(0, culture);
                InvoiceCount = "0";
                ActiveTables = "0 / 0";
                BestSellerName = "Không tải được dữ liệu";
                FocusItems = new ObservableCollection<string>
                {
                    "Không thể tải dữ liệu dashboard từ cơ sở dữ liệu.",
                    string.Format("Lỗi kết nối hoặc truy vấn: {0}", ex.Message)
                };

                LoadRevenueChartData(new List<HoaDon>(), reportDate, now, culture);
            }
        }

        private void LoadRevenueChartData(IEnumerable<HoaDon> paidInvoices, DateTime reportDate, DateTime now, CultureInfo culture)
        {
            Dictionary<int, decimal> hourlyRevenue = paidInvoices
                .GroupBy(hd => hd.NgayLap.Hour)
                .ToDictionary(group => group.Key, group => group.Sum(hd => hd.TongTien));

            List<int> hours = ResolveChartHours(hourlyRevenue, reportDate, now);
            List<decimal> values = hours
                .Select(hour => hourlyRevenue.ContainsKey(hour) ? hourlyRevenue[hour] : 0)
                .ToList();

            decimal maxValue = RoundChartMax(values.Count > 0 ? values.Max() : 0);
            const double chartWidth = 540.0;
            const double chartHeight = 120.0;
            double stepX = chartWidth / (values.Count - 1);

            RevenueHourLabels = new ObservableCollection<string>(hours.Select(hour => string.Format("{0:00}:00", hour)));
            RevenueAxisTopLabel = FormatCompactCurrency(maxValue, culture);
            RevenueAxisHighLabel = FormatCompactCurrency(maxValue * 2 / 3, culture);
            RevenueAxisLowLabel = FormatCompactCurrency(maxValue / 3, culture);
            List<Point> points = new List<Point>();

            for (int i = 0; i < values.Count; i++)
            {
                decimal value = values[i];
                double x = i * stepX;
                double y = chartHeight - ((double)(value / maxValue) * chartHeight);
                points.Add(new Point(x, y));
            }

            RevenueLineGeometry = CreateSmoothLineGeometry(points);
            RevenueAreaGeometry = CreateSmoothAreaGeometry(points, chartHeight, chartWidth);
        }

        private static ObservableCollection<string> BuildFocusItems(
            DateTime reportDate,
            CultureInfo culture,
            int invoiceCount,
            int unpaidInvoiceCount,
            int activeTableCount,
            int totalTableCount,
            int lowStockCount)
        {
            ObservableCollection<string> items = new ObservableCollection<string>();
            string reportDateText = reportDate.ToString("dd/MM/yyyy", culture);

            if (invoiceCount == 0)
            {
                items.Add(string.Format("Chưa có hóa đơn trong ngày {0}.", reportDateText));
            }
            else if (unpaidInvoiceCount > 0)
            {
                items.Add(string.Format("Có {0} hóa đơn chưa thanh toán trong ngày {1}.", unpaidInvoiceCount.ToString("N0", culture), reportDateText));
            }
            else
            {
                items.Add(string.Format("Tất cả hóa đơn trong ngày {0} đã thanh toán.", reportDateText));
            }

            items.Add(string.Format("Bàn đang phục vụ: {0}/{1}.", activeTableCount.ToString("N0", culture), totalTableCount.ToString("N0", culture)));

            if (lowStockCount > 0)
            {
                items.Add(string.Format("Có {0} nguyên liệu ở mức cảnh báo tồn kho.", lowStockCount.ToString("N0", culture)));
            }
            else
            {
                items.Add("Không có nguyên liệu ở mức cảnh báo tồn kho.");
            }

            return items;
        }

        private static string ResolveBestSellerName(QL_CoffeeTeaEntities context, DateTime reportStart, DateTime reportEnd)
        {
            var bestSeller = context.ChiTietHoaDons
                .Include(cthd => cthd.HoaDon)
                .Include(cthd => cthd.Mon)
                .Where(cthd => cthd.HoaDon.NgayLap >= reportStart && cthd.HoaDon.NgayLap < reportEnd)
                .ToList()
                .GroupBy(cthd => cthd.Mon != null ? cthd.Mon.TenMon : cthd.MaMon)
                .Select(group => new
                {
                    Name = group.Key,
                    Quantity = group.Sum(cthd => cthd.SoLuong)
                })
                .OrderByDescending(item => item.Quantity)
                .ThenBy(item => item.Name)
                .FirstOrDefault();

            if (bestSeller == null || string.IsNullOrWhiteSpace(bestSeller.Name))
            {
                return "Chưa có dữ liệu";
            }

            return string.Format("{0} ({1})", bestSeller.Name.Trim(), bestSeller.Quantity);
        }

        private static List<int> ResolveChartHours(Dictionary<int, decimal> hourlyRevenue, DateTime reportDate, DateTime now)
        {
            if (hourlyRevenue.Count > 0)
            {
                int minHour = hourlyRevenue.Keys.Min();
                int maxHour = hourlyRevenue.Keys.Max();
                List<int> hours = Enumerable.Range(minHour, maxHour - minHour + 1).ToList();

                if (hours.Count == 1)
                {
                    if (hours[0] < 22)
                    {
                        hours.Add(hours[0] + 1);
                    }
                    else
                    {
                        hours.Insert(0, hours[0] - 1);
                    }
                }

                return hours;
            }

            int startHour;
            int endHour;

            if (reportDate.Date == now.Date && now.Hour >= 12 && now.Hour < 18)
            {
                startHour = 12;
                endHour = 17;
            }
            else if (reportDate.Date == now.Date && now.Hour >= 18)
            {
                startHour = 18;
                endHour = 22;
            }
            else
            {
                startHour = 7;
                endHour = 12;
            }

            return Enumerable.Range(startHour, endHour - startHour + 1).ToList();
        }

        private static bool IsPaidInvoice(HoaDon invoice)
        {
            return invoice != null && IsPaidStatus(invoice.TrangThai);
        }

        private static bool IsPaidStatus(string status)
        {
            return !string.IsNullOrWhiteSpace(status)
                && status.IndexOf("đã thanh toán", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsActiveTableStatus(string status)
        {
            return !string.IsNullOrWhiteSpace(status)
                && status.IndexOf("đang phục vụ", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FormatCurrency(decimal value, CultureInfo culture)
        {
            return string.Format(culture, "{0:N0} VNĐ", value);
        }

        private static string FormatCompactCurrency(decimal value, CultureInfo culture)
        {
            if (value >= 1000000)
            {
                return string.Format(culture, "{0:0.#}M", value / 1000000);
            }

            if (value >= 1000)
            {
                return string.Format(culture, "{0:0.#}K", value / 1000);
            }

            return value.ToString("N0", culture);
        }

        private static decimal RoundChartMax(decimal maxValue)
        {
            if (maxValue <= 0)
            {
                return 1;
            }

            decimal unit;

            if (maxValue >= 1000000)
            {
                unit = 1000000;
            }
            else if (maxValue >= 100000)
            {
                unit = 100000;
            }
            else if (maxValue >= 10000)
            {
                unit = 10000;
            }
            else
            {
                unit = 1000;
            }

            return Math.Ceiling(maxValue / unit) * unit;
        }

        private static bool IsStoreOpen(DateTime now)
        {
            TimeSpan currentTime = now.TimeOfDay;
            TimeSpan openTime = new TimeSpan(6, 30, 0);
            TimeSpan closeTime = new TimeSpan(22, 30, 0);
            return currentTime >= openTime && currentTime <= closeTime;
        }

        private static string ResolveShift(DateTime now)
        {
            if (now.Hour < 12)
            {
                return "Ca sáng";
            }

            if (now.Hour < 18)
            {
                return "Ca chiều";
            }

            return "Ca tối";
        }

        private NhanVien ResolveCurrentAccount()
        {
            if (string.IsNullOrWhiteSpace(_authenticatedUser?.MaNhanVien))
            {
                return _authenticatedUser;
            }

            try
            {
                using (QL_CoffeeTeaEntities context = new QL_CoffeeTeaEntities())
                {
                    NhanVien dbUser = context.NhanViens
                        .Include(nv => nv.VaiTro)
                        .FirstOrDefault(nv => nv.MaNhanVien == _authenticatedUser.MaNhanVien);

                    return dbUser ?? _authenticatedUser;
                }
            }
            catch (Exception)
            {
                return _authenticatedUser;
            }
        }

        private string ResolveAccountName(NhanVien account, CultureInfo culture)
        {
            if (!string.IsNullOrWhiteSpace(account?.HoTen))
            {
                return account.HoTen.Trim();
            }

            string fallbackUserName = !string.IsNullOrWhiteSpace(account?.TenDangNhap)
                ? account.TenDangNhap
                : Environment.UserName;

            return BuildDisplayName(fallbackUserName, culture);
        }

        private string ResolveAccountRole(NhanVien account)
        {
            string roleName = account?.VaiTro != null ? account.VaiTro.TenVaiTro : null;
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                return roleName.Trim();
            }

            if (string.Equals(account?.MaVaiTro, "VT01", StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }

            if (string.Equals(account?.MaVaiTro, "VT02", StringComparison.OrdinalIgnoreCase))
            {
                return "Quản lý";
            }

            if (string.Equals(account?.MaVaiTro, "VT03", StringComparison.OrdinalIgnoreCase))
            {
                return "Nhân viên";
            }

            return "Không xác định";
        }

        private string ResolveSignInSource(NhanVien account)
        {
            string userName = account != null ? account.TenDangNhap : null;
            string employeeCode = account != null ? account.MaNhanVien : null;

            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(employeeCode))
            {
                return string.Format("{0} ({1})", userName.Trim(), employeeCode.Trim());
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName.Trim();
            }

            return string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        private static string ResolveInitial(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "U";
            }

            return displayName.Substring(0, 1).ToUpperInvariant();
        }

        private static string BuildDisplayName(string rawUserName, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(rawUserName))
            {
                return "Người dùng hệ thống";
            }

            string normalized = rawUserName
                .Replace(".", " ")
                .Replace("_", " ")
                .Replace("-", " ")
                .Trim();

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "Người dùng hệ thống";
            }

            return culture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
        }

        private static Geometry CreateSmoothLineGeometry(IList<Point> points)
        {
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(points[0], false, false);

                for (int i = 1; i < points.Count; i++)
                {
                    Point previous = points[i - 1];
                    Point current = points[i];
                    double midX = (previous.X + current.X) / 2;

                    context.BezierTo(
                        new Point(midX, previous.Y),
                        new Point(midX, current.Y),
                        current,
                        true,
                        true);
                }
            }

            geometry.Freeze();
            return geometry;
        }

        private static Geometry CreateSmoothAreaGeometry(IList<Point> points, double chartHeight, double chartWidth)
        {
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(new Point(0, chartHeight), true, true);
                context.LineTo(points[0], true, true);

                for (int i = 1; i < points.Count; i++)
                {
                    Point previous = points[i - 1];
                    Point current = points[i];
                    double midX = (previous.X + current.X) / 2;

                    context.BezierTo(
                        new Point(midX, previous.Y),
                        new Point(midX, current.Y),
                        current,
                        true,
                        true);
                }

                context.LineTo(new Point(chartWidth, chartHeight), true, true);
            }

            geometry.Freeze();
            return geometry;
        }
    }
}
