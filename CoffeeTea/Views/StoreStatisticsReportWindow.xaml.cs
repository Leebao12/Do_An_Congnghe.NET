using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;
using CoffeeTea.Report;
using ReportInputData = CoffeeTea.ViewModels.StatisticsReportData;
using StatisticsCrystalReport = CoffeeTea.Report.StatisticsReportData;

namespace CoffeeTea.Views
{
    public partial class StoreStatisticsReportWindow : Window
    {
        private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN"); // Định dạng Việt Nam để hiển thị ngày tháng và số

        private readonly ReportInputData _reportData; // Dữ liệu đầu vào cho báo cáo, chứa thông tin về khoảng thời gian, tổng doanh thu, số hóa đơn, v.v.
        private readonly CrystalReportViewer _viewer; // Điều khiển xem báo cáo Crystal Report, được sử dụng để hiển thị báo cáo thống kê trong cửa sổ
        private ReportDocument _reportDocument; // Đối tượng ReportDocument đại diện cho báo cáo Crystal Report, được sử dụng để thiết lập nguồn dữ liệu và tham số cho báo cáo

        public StoreStatisticsReportWindow(ReportInputData reportData)
        {
            if (reportData == null)
            {
                throw new ArgumentNullException(nameof(reportData));
            }

            InitializeComponent();

            _reportData = reportData;
            _viewer = new CrystalReportViewer
            {
                Dock = System.Windows.Forms.DockStyle.Fill, 
                ToolPanelView = ToolPanelViewType.None, 
                ShowLogo = false,
                ShowGroupTreeButton = false, 
                ReuseParameterValuesOnRefresh = true //Giữ nguyên giá trị tham số khi làm mới báo cáo, giúp người dùng không phải nhập lại tham số sau mỗi lần làm mới
            };

            ReportHost.Child = _viewer; //Đặt CrystalReportViewer làm con của ReportHost, cho phép hiển thị báo cáo trong cửa sổ WPF
            PeriodTextBlock.Text = string.Format(
                VietnameseCulture,
                "Từ ngày {0:dd/MM/yyyy} đến ngày {1:dd/MM/yyyy} - {2:N0} hóa đơn",
                _reportData.FromDate.Date,
                _reportData.ToDate.Date,
                _reportData.TotalInvoices);

            Loaded += StoreStatisticsReportWindow_Loaded; //Đăng ký sự kiện Loaded để tải báo cáo khi cửa sổ được hiển thị
            Closed += StoreStatisticsReportWindow_Closed; //Đăng ký sự kiện Closed để giải phóng tài nguyên khi cửa sổ bị đóng
        }

        private void StoreStatisticsReportWindow_Loaded(object sender, RoutedEventArgs e) // Sự kiện được gọi khi cửa sổ đã được tải và hiển thị, tại đây sẽ gọi phương thức LoadCrystalReport để thiết lập và hiển thị báo cáo thống kê
        {
            LoadCrystalReport();
        }

        private void LoadCrystalReport()
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture; 
            var oldUICulture = Thread.CurrentThread.CurrentUICulture; 

            try
            {
                Thread.CurrentThread.CurrentCulture = VietnameseCulture;
                Thread.CurrentThread.CurrentUICulture = VietnameseCulture;

                _reportDocument = new StatisticsCrystalReport(); // Tạo một instance của báo cáo thống kê, đây là lớp được tạo ra từ file .rpt của Crystal Reports, chứa định nghĩa về cấu trúc và thiết kế của báo cáo

                dsThongKe dataSet = CreateReportDataSet(); // Tạo một DataSet chứa dữ liệu cho báo cáo, phương thức này sẽ chuyển đổi dữ liệu từ ReportInputData thành định dạng mà Crystal Reports có thể sử dụng để hiển thị trong báo cáo
                _reportDocument.SetDataSource(dataSet);
                _reportDocument.Database.Tables["HoaDon"].SetDataSource((DataTable)dataSet.HoaDon); // Thiết lập nguồn dữ liệu cho bảng "HoaDon" trong báo cáo, đảm bảo rằng dữ liệu hóa đơn được hiển thị chính xác trong báo cáo
                CompactDetailSection(); // Gọi phương thức để điều chỉnh phần chi tiết của báo cáo, giúp hiển thị nhiều hóa đơn hơn trên một trang mà không bị cắt bớt thông tin quan trọng


                SetParameterIfExists("pTongDoanhThu", _reportData.TotalRevenue); // Thiết lập giá trị tham số "pTongDoanhThu" trong báo cáo nếu tham số này tồn tại, giúp hiển thị tổng doanh thu trong báo cáo
                SetParameterIfExists("pSoHoaDon", _reportData.TotalInvoices);
                SetParameterIfExists("pTBHoaDon", _reportData.AveragePerInvoice);
                SetParameterIfExists("pTuNgay", _reportData.FromDate.Date);
                SetParameterIfExists("pDenNgay", _reportData.ToDate.Date);

                ParameterFields parameterFields = CreateViewerParameterFields(); 
                _viewer.ParameterFieldInfo = parameterFields; // Thiết lập thông tin tham số cho CrystalReportViewer, giúp hiển thị các tham số đã tạo trên giao diện người dùng của báo cáo
                _viewer.ReportSource = _reportDocument;
                _viewer.Refresh(); // Làm mới báo cáo để hiển thị dữ liệu và tham số đã thiết lập, đảm bảo rằng người dùng thấy được thông tin cập nhật khi mở báo cáo
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tải báo cáo thống kê.\n\nChi tiết lỗi:\n" + ex.Message,
                    "Lỗi Crystal Report",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
                Thread.CurrentThread.CurrentUICulture = oldUICulture;
            }
        }

       

        private dsThongKe CreateReportDataSet()
        {
            var dataSet = new dsThongKe();

            foreach (var item in _reportData.Items ?? Enumerable.Empty<CoffeeTea.ViewModels.StatisticsReportInvoiceItem>())
            {
                dataSet.HoaDon.AddHoaDonRow(
                    item.No,
                    Limit(item.InvoiceId, 10),
                    ParseCreatedAt(item.CreatedAt),
                    Limit(item.StaffName, 100),
                    Limit(item.TableName, 50),
                    Limit(item.PaymentMethod, 50),
                    item.TotalAmount,
                    Limit(item.Status, 30));
            }

            return dataSet;
        }

        private ParameterFields CreateViewerParameterFields()
        {
            var parameterFields = new ParameterFields();

            AddParameter(parameterFields, "pTongDoanhThu", _reportData.TotalRevenue);
            AddParameter(parameterFields, "pSoHoaDon", _reportData.TotalInvoices);
            AddParameter(parameterFields, "pTBHoaDon", _reportData.AveragePerInvoice);
            AddParameter(parameterFields, "pTuNgay", _reportData.FromDate.Date);
            AddParameter(parameterFields, "pDenNgay", _reportData.ToDate.Date);

            return parameterFields;
        }

        private void AddParameter(ParameterFields parameterFields, string parameterName, object value)
        {
            var parameterField = new ParameterField
            {
                Name = parameterName
            };

            var parameterValue = new ParameterDiscreteValue
            {
                Value = value
            };

            parameterField.CurrentValues.Add(parameterValue);
            parameterFields.Add(parameterField);
        }

        private static DateTime ParseCreatedAt(string value)
        {
            DateTime result;
            string[] formats =
            {
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy",
                "d/M/yyyy HH:mm",
                "d/M/yyyy"
            };

            if (DateTime.TryParseExact(value, formats, VietnameseCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            if (DateTime.TryParse(value, VietnameseCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return DateTime.Today;
        }

        private static string Limit(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private void SetParameterIfExists(string parameterName, object value)
        {
            bool exists = _reportDocument.DataDefinition.ParameterFields
                .Cast<ParameterFieldDefinition>()
                .Any(parameter => parameter.Name == parameterName);

            if (exists)
            {
                _reportDocument.SetParameterValue(parameterName, value);
            }
        }

        private void CompactDetailSection()
        {
            Section detailSection = _reportDocument.ReportDefinition.Sections
                .Cast<Section>()
                .FirstOrDefault(section => section.ReportObjects
                    .Cast<ReportObject>()
                    .Any(reportObject => reportObject.Name == "STT1"
                        || reportObject.Name == "MaHD1"
                        || reportObject.Name == "TongTien1"));

            if (detailSection == null)
            {
                return;
            }

            foreach (ReportObject reportObject in detailSection.ReportObjects)
            {
                reportObject.ObjectFormat.EnableCanGrow = false;
                reportObject.ObjectFormat.EnableKeepTogether = false;
            }

            detailSection.Height = 360;
            detailSection.SectionFormat.EnableKeepTogether = false;
            detailSection.SectionFormat.EnableNewPageBefore = false;
            detailSection.SectionFormat.EnableNewPageAfter = false;
            detailSection.SectionFormat.EnablePrintAtBottomOfPage = false;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewer.ReportSource != null)
            {
                _viewer.PrintReport();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StoreStatisticsReportWindow_Closed(object sender, EventArgs e)
        {
            _viewer.ReportSource = null;
            ReportHost.Child = null;

            if (_reportDocument != null)
            {
                _reportDocument.Close();
                _reportDocument.Dispose();
                _reportDocument = null;
            }
        }
    }
}
