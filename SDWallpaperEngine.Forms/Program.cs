using Microsoft.Extensions.Configuration;
using SDWallpaperEngine.Shared;

namespace SDWallpaperEngine.Forms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var settings = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build()
                .Get<AppSettings>() ?? new AppSettings();

            ApplicationConfiguration.Initialize();
            Application.Run(new WindowMain(settings));
        }
    }
}