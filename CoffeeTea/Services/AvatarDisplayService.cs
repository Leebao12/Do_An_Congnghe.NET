using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoffeeTea.Services
{
    public static class AvatarDisplayService
    {
        public const string DefaultAvatarRelativePath = "Images/Employees/default-avatar.png";
        public const string EmployeeAvatarRelativeFolder = "Images/Employees";

        public static ImageSource LoadAvatarImage(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
            {
                return null;
            }

            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(absolutePath, UriKind.Absolute);
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }

        public static string ResolveAvatarPath(string relativePath)
        {
            string normalizedPath = NormalizeRelativePath(relativePath);

            return ResolveApplicationPath(string.IsNullOrWhiteSpace(normalizedPath)
                ? DefaultAvatarRelativePath
                : normalizedPath);
        }

        public static string ResolveApplicationPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                relativePath = DefaultAvatarRelativePath;
            }

            if (Path.IsPathRooted(relativePath))
            {
                return relativePath;
            }

            string normalizedPath = relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            return Path.Combine(GetProjectRootDirectory(), normalizedPath);
        }

        public static string NormalizeRelativePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim()
                .TrimStart('/', '\\')
                .Replace('\\', '/');
        }

        private static string GetProjectRootDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "CoffeeTea.csproj")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
