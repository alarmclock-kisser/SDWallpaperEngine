namespace SDWallpaperEngine.Shared
{
    public class AppSettings
    {
        public string ComfyUiApiUrl { get; set; } = "http://localhost:8000";
        public string ComfyUiApiKey { get; set; } = "";
        public string ComfyUiExePath { get; set; } = @"%appdata%\Local\Programs\ComfyUI\ComfyUI.exe";
        public string ComfyUiLaunchArguments { get; set; } = "";
        public string ComfyUiWorkflowTemplatePath { get; set; } = @"Ressources\fast_wallpaper_gen.json";
        public string PositivePrompt { get; set; } = "evolve and derive the given image through iteration, keep the same color palette and try to rotate or morph visible features or structures to reshape them slightly or apply another artstyle (used for wallpaper engine)";
        public string NegativePrompt { get; set; } = "blurry, jpeg artifacts, noise";
        public string OutputDirectory { get; set; } = "Output";
        public int MaxImagesKeep { get; set; } = 16;
        public int MaxTimeoutSeconds { get; set; } = 180;
        public int MaxRetries { get; set; } = 3;



    }
}
