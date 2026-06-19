using System;

namespace CoffeeTea.Models
{
    public class SoftwareSettingsModel
    {
        public string StoreDisplayName { get; set; }

        public string StoreAddress { get; set; }

        public string Hotline { get; set; }

        public string OpenTime { get; set; }

        public string CloseTime { get; set; }

        public bool IsDarkTheme { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        public static SoftwareSettingsModel CreateDefault()
        {
            return new SoftwareSettingsModel
            {
                StoreDisplayName = "CoffeeTea",
                StoreAddress = "Chi nhánh mặc định - vui lòng cập nhật địa chỉ thực tế.",
                Hotline = "0901 000 001",
                OpenTime = "06:30",
                CloseTime = "22:30",
                IsDarkTheme = false,
                LastUpdatedBy = "Hệ thống",
                LastUpdatedAt = DateTime.Now
            };
        }
    }
}
