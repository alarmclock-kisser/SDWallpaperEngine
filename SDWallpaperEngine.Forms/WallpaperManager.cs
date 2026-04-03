using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SDWallpaperEngine.Forms
{
    internal static class WallpaperManager
    {
        private const int SpiSetDeskWallpaper = 0x0014;
        private const int SpifUpdateIniFile = 0x01;
        private const int SpifSendChange = 0x02;

        public static string ResolveCurrentWallpaperFilePath()
        {
            var registryWallpaper = GetRegistryWallpaperPath();
            if (File.Exists(registryWallpaper))
            {
                return registryWallpaper;
            }

            var transcodedWallpaper = GetTranscodedWallpaperPath();
            if (File.Exists(transcodedWallpaper))
            {
                return EnsureTempImageCopy(transcodedWallpaper);
            }

            throw new FileNotFoundException("Could not resolve the current Windows wallpaper file.");
        }

        public static void SetWallpaper(string wallpaperFilePath)
        {
            if (string.IsNullOrWhiteSpace(wallpaperFilePath))
            {
                throw new ArgumentException("Wallpaper path is required.", nameof(wallpaperFilePath));
            }

            if (!File.Exists(wallpaperFilePath))
            {
                throw new FileNotFoundException("Wallpaper image was not found.", wallpaperFilePath);
            }

            if (!SystemParametersInfo(SpiSetDeskWallpaper, 0, wallpaperFilePath, SpifUpdateIniFile | SpifSendChange))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static string CreateWallpaperCompatibleCopy(byte[] imageBytes, string outputDirectory, int maxImagesKeep)
        {
            var targetDirectory = ResolveOutputDirectoryPath(outputDirectory);
            Directory.CreateDirectory(targetDirectory);

            var targetPath = Path.Combine(targetDirectory, $"wallpaper_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.bmp");

            using var inputStream = new MemoryStream(imageBytes);
            using var image = Image.FromStream(inputStream);
            using var bitmap = new Bitmap(image);
            bitmap.Save(targetPath, ImageFormat.Bmp);

            TrimOldFiles(targetDirectory, maxImagesKeep);

            return targetPath;
        }

        public static string ResolveOutputDirectoryPath(string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                outputDirectory = "Output";
            }

            if (Path.IsPathRooted(outputDirectory))
            {
                return outputDirectory;
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, outputDirectory));
        }

        private static void TrimOldFiles(string directory, int maxImagesKeep)
        {
            maxImagesKeep = Math.Max(1, maxImagesKeep);

            var files = Directory.EnumerateFiles(directory, "wallpaper_*.bmp", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            foreach (var file in files.Skip(maxImagesKeep))
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                }
            }
        }

        private static string GetRegistryWallpaperPath()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
            var wallpaper = key?.GetValue("WallPaper") as string;
            return string.IsNullOrWhiteSpace(wallpaper) ? string.Empty : wallpaper;
        }

        private static string GetTranscodedWallpaperPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes\TranscodedWallpaper");
        }

        private static string EnsureTempImageCopy(string sourcePath)
        {
            var extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".jpg";
            }

            var targetDirectory = Path.Combine(Path.GetTempPath(), "SDWallpaperEngine", "WallpaperCache");
            Directory.CreateDirectory(targetDirectory);

            var targetPath = Path.Combine(targetDirectory, $"wallpaper_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}{extension}");
            File.Copy(sourcePath, targetPath, overwrite: true);
            return targetPath;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    }
}
