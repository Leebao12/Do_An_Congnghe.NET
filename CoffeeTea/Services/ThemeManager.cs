using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CoffeeTea.Services
{
    public static class ThemeManager
    {
        private static readonly IReadOnlyDictionary<string, string> LightBrushes = new Dictionary<string, string>
        {
            { "Brush.Primary", "#7EC8E3" },
            { "Brush.PrimaryDark", "#4FA7C9" },
            { "Brush.PrimaryLight", "#EAF8FD" },
            { "Brush.WindowBackground", "#F6FCFE" },
            { "Brush.Sidebar", "#DDF4FB" },
            { "Brush.Card", "#FFFFFF" },
            { "Brush.BorderSoft", "#C9E8F3" },
            { "Brush.BorderStrong", "#97D1E5" },
            { "Brush.TextPrimary", "#21495C" },
            { "Brush.TextSecondary", "#5B7C8C" },
            { "Brush.TextWhite", "#FFFFFF" },
            { "Brush.Success", "#A8E6CF" },
            { "Brush.Warning", "#FFF7E8" },
            { "Brush.Danger", "#F56C6C" },
            { "Brush.Info", "#D9F3FF" },
            { "Brush.Hover", "#C9ECF8" },
            { "Brush.Selected", "#B8E4F4" },
            { "Brush.SidebarCard", "#F1FBFF" },
            { "Brush.DangerSoft", "#FDEBEC" },
            { "Brush.DataGridRow", "#FFFFFF" },
            { "Brush.DataGridAlternateRow", "#F8FDFF" },
            { "Brush.DataGridRowBorder", "#EDF7FB" },
            { "Brush.ChartCardBackground", "#D9EEF6" },
            { "Brush.ChartCardBorder", "#C6E3ED" },
            { "Brush.ChartTitle", "#2E5565" },
            { "Brush.ChartAccentDot", "#FF5B5B" },
            { "Brush.ChartGridLine", "#BFDCE6" },
            { "Brush.ChartGridBaseLine", "#94B9C7" },
            { "Brush.ChartArea", "#558DBFD1" },
            { "Brush.ChartLine", "#5F9FB7" }
        };

        private static readonly IReadOnlyDictionary<string, string> DarkBrushes = new Dictionary<string, string>
        {
            { "Brush.Primary", "#5DA8C6" },
            { "Brush.PrimaryDark", "#7CC6E1" },
            { "Brush.PrimaryLight", "#214352" },
            { "Brush.WindowBackground", "#111A22" },
            { "Brush.Sidebar", "#162631" },
            { "Brush.Card", "#1C2D3A" },
            { "Brush.BorderSoft", "#2F4A5A" },
            { "Brush.BorderStrong", "#4C7387" },
            { "Brush.TextPrimary", "#E7F4FA" },
            { "Brush.TextSecondary", "#B3D0DD" },
            { "Brush.TextWhite", "#FFFFFF" },
            { "Brush.Success", "#2E6F57" },
            { "Brush.Warning", "#7A5A2A" },
            { "Brush.Danger", "#FF8A8A" },
            { "Brush.Info", "#294A5B" },
            { "Brush.Hover", "#274353" },
            { "Brush.Selected", "#32586B" },
            { "Brush.SidebarCard", "#203443" },
            { "Brush.DangerSoft", "#5B2C35" },
            { "Brush.DataGridRow", "#1E313F" },
            { "Brush.DataGridAlternateRow", "#223746" },
            { "Brush.DataGridRowBorder", "#2C4757" },
            { "Brush.ChartCardBackground", "#203744" },
            { "Brush.ChartCardBorder", "#35586A" },
            { "Brush.ChartTitle", "#D8F1FC" },
            { "Brush.ChartAccentDot", "#FF8F8F" },
            { "Brush.ChartGridLine", "#3E6173" },
            { "Brush.ChartGridBaseLine", "#6B93A6" },
            { "Brush.ChartArea", "#5590C7DB" },
            { "Brush.ChartLine", "#8CCFE6" }
        };

        public static void ApplyTheme(bool isDarkTheme)
        {
            Application app = Application.Current;
            if (app == null)
            {
                return;
            }

            IReadOnlyDictionary<string, string> themeBrushes = isDarkTheme ? DarkBrushes : LightBrushes;
            foreach (KeyValuePair<string, string> item in themeBrushes)
            {
                UpdateBrushColor(app, item.Key, item.Value);
            }

            UpdateBrushColor(app, SystemColors.ControlTextBrushKey, themeBrushes["Brush.TextPrimary"]);
            UpdateBrushColor(app, SystemColors.GrayTextBrushKey, themeBrushes["Brush.TextSecondary"]);
            UpdateBrushColor(app, SystemColors.HighlightBrushKey, themeBrushes["Brush.Primary"]);
            UpdateBrushColor(app, SystemColors.HighlightTextBrushKey, themeBrushes["Brush.TextWhite"]);
        }

        private static void UpdateBrushColor(Application app, object key, string colorHex)
        {
            if (app == null)
            {
                return;
            }

            Color parsedColor = (Color)ColorConverter.ConvertFromString(colorHex);
            SolidColorBrush replacementBrush = new SolidColorBrush(parsedColor);
            ReplaceBrushInDictionaries(app.Resources, key, replacementBrush);
        }

        private static bool ReplaceBrushInDictionaries(ResourceDictionary dictionary, object key, SolidColorBrush replacementBrush)
        {
            if (dictionary == null)
            {
                return false;
            }

            if (dictionary.Contains(key))
            {
                dictionary[key] = replacementBrush;
                return true;
            }

            foreach (ResourceDictionary mergedDictionary in dictionary.MergedDictionaries)
            {
                if (ReplaceBrushInDictionaries(mergedDictionary, key, replacementBrush))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
