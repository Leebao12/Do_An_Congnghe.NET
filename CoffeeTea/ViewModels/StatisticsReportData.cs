using System;
using System.Collections.Generic;

namespace CoffeeTea.ViewModels
{
    public class StatisticsReportData
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DateTime GeneratedAt { get; set; }

        public decimal TotalRevenue { get; set; }

        public int TotalInvoices { get; set; }

        public decimal AveragePerInvoice { get; set; }

        public IList<StatisticsReportInvoiceItem> Items { get; set; } //Danh sách các hóa ??n trong kho?ng th?i gian

        public StatisticsReportData() //Kh?i t?o m?c ??nh v?i ngày hi?n t?i
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
            GeneratedAt = DateTime.Now;
            Items = new List<StatisticsReportInvoiceItem>();
        }

        public StatisticsReportData(DateTime fromDate, DateTime toDate) //Kh?i t?o v?i kho?ng th?i gian c? th?
            : this()
        {
            FromDate = fromDate;
            ToDate = toDate;
        }
    }

    public class StatisticsReportInvoiceItem
    {
        public int No { get; set; }

        public string InvoiceId { get; set; }

        public string CreatedAt { get; set; }

        public string TableName { get; set; }

        public string StaffName { get; set; }

        public string PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }
    }
}
