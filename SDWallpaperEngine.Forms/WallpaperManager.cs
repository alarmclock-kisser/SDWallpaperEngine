using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SDWallpaperEngine.Forms
{
    internal static class WallpaperManager
    {
        private const int SpiSetDeskWallpaper = 0x0014;
        private const int SpifUpdateIniFile = 0x01;
        private const int SpifSendChange = 0x02;
        private static readonly Guid DesktopWallpaperClsid = new("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD");

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

        public static IReadOnlyDictionary<string, string> ResolveCurrentWallpaperFilePathsPerMonitor()
        {
            var wallpapers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var desktopWallpaper = TryCreateDesktopWallpaper();

            if (desktopWallpaper is not null)
            {
                try
                {
                    var monitorCount = desktopWallpaper.GetMonitorDevicePathCount();
                    for (uint index = 0; index < monitorCount; index++)
                    {
                        var monitorId = desktopWallpaper.GetMonitorDevicePathAt(index);
                        var wallpaperPath = desktopWallpaper.GetWallpaper(monitorId);

                        if (!string.IsNullOrWhiteSpace(wallpaperPath) && File.Exists(wallpaperPath))
                        {
                            wallpapers[monitorId] = wallpaperPath;
                        }
                    }
                }
                finally
                {
                    ReleaseComObject(desktopWallpaper);
                }
            }

            if (wallpapers.Count == 0)
            {
                wallpapers[string.Empty] = ResolveCurrentWallpaperFilePath();
            }

            return wallpapers;
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

        public static void SetWallpapersPerMonitor(IReadOnlyDictionary<string, string> wallpapersByMonitor)
        {
            if (wallpapersByMonitor is null || wallpapersByMonitor.Count == 0)
            {
                throw new ArgumentException("At least one wallpaper path is required.", nameof(wallpapersByMonitor));
            }

            var firstExistingPath = wallpapersByMonitor.Values.FirstOrDefault(File.Exists);
            if (string.IsNullOrWhiteSpace(firstExistingPath))
            {
                throw new FileNotFoundException("No saved wallpaper image was found to restore.");
            }

            var desktopWallpaper = TryCreateDesktopWallpaper();
            if (desktopWallpaper is not null)
            {
                try
                {
                    var restoredCount = 0;
                    var monitorCount = desktopWallpaper.GetMonitorDevicePathCount();
                    for (uint index = 0; index < monitorCount; index++)
                    {
                        var monitorId = desktopWallpaper.GetMonitorDevicePathAt(index);
                        if (!wallpapersByMonitor.TryGetValue(monitorId, out var monitorWallpaperPath))
                        {
                            continue;
                        }

                        if (!File.Exists(monitorWallpaperPath))
                        {
                            continue;
                        }

                        desktopWallpaper.SetWallpaper(monitorId, monitorWallpaperPath);
                        restoredCount++;
                    }

                    if (restoredCount > 0)
                    {
                        return;
                    }
                }
                finally
                {
                    ReleaseComObject(desktopWallpaper);
                }
            }

            SetWallpaper(firstExistingPath);
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

        private static IDesktopWallpaper? TryCreateDesktopWallpaper()
        {
            try
            {
                var desktopWallpaperType = Type.GetTypeFromCLSID(DesktopWallpaperClsid, throwOnError: false);
                if (desktopWallpaperType is null)
                {
                    return null;
                }

                return Activator.CreateInstance(desktopWallpaperType) as IDesktopWallpaper;
            }
            catch
            {
                return null;
            }
        }

        private static void ReleaseComObject(object comObject)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<object>())
            {
                Marshal.FinalReleaseComObject(comObject);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [ComImport]
        [Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDesktopWallpaper
        {
            void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetMonitorDevicePathAt(uint monitorIndex);

            uint GetMonitorDevicePathCount();
        }
    }
}
