using SDWallpaperEngine.ComfyUi;
using SDWallpaperEngine.Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace SDWallpaperEngine.Forms
{
    public partial class WindowMain : Form
    {
        private const string BaseTitle = "SDWall-E (Configuration)";

        private readonly AppSettings _settings;
        private readonly ComfyUiApiClient _client;
        private readonly ComfyUiWallpaperAutomationService _automationService;
        private readonly IProgress<double> _progress;
        private readonly Queue<string> _generatedWallpapers = new();
        private GifRecordingSession? _gifRecordingSession;
        private CancellationTokenSource? _loopCancellation;
        private Task? _loopTask;
        private bool _isClosing;
        private bool _suppressEnabledChanged;
        private bool _suppressSettingsChangeEvents;
        private bool _suppressRecordSync;
        private string? _savedWallpaperPath;
        private int _consecutiveFailures;
        private const int MaxLogEntries = 500;

        public WindowMain(AppSettings settings)
        {
            InitializeComponent();

            _settings = settings ?? new AppSettings();
            _client = new ComfyUiApiClient(_settings);
            _automationService = new ComfyUiWallpaperAutomationService(_settings, _client);
            _progress = new Progress<double>(UpdateProgress);

            var maxImages = Math.Clamp(_settings.MaxImagesKeep, (int)numericUpDown_maxImages.Minimum, (int)numericUpDown_maxImages.Maximum);
            numericUpDown_maxImages.Value = maxImages;

            Text = BaseTitle;
        }

        private void WindowMain_Shown(object? sender, EventArgs e)
        {
            var outputDirectory = WallpaperManager.ResolveOutputDirectoryPath(_settings.OutputDirectory);
            Directory.CreateDirectory(outputDirectory);
            AddLog($"Output directory: {outputDirectory}");

            TryShowCurrentWallpaperPreview();

            if (checkBox_enabled.Checked)
            {
                if (!SaveCurrentWallpaperForRestore())
                {
                    _suppressEnabledChanged = true;
                    checkBox_enabled.Checked = false;
                    _suppressEnabledChanged = false;
                    return;
                }

                StartLoop();
            }
        }

        private async void CheckBoxEnabled_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isClosing || _suppressEnabledChanged)
            {
                return;
            }

            if (checkBox_enabled.Checked)
            {
                if (!SaveCurrentWallpaperForRestore())
                {
                    _suppressEnabledChanged = true;
                    checkBox_enabled.Checked = false;
                    _suppressEnabledChanged = false;
                    return;
                }

                StartLoop();
            }
            else
            {
                await StopLoopAsync().ConfigureAwait(true);
            }
        }

        private bool SaveCurrentWallpaperForRestore()
        {
            try
            {
                var currentWallpaper = WallpaperManager.ResolveCurrentWallpaperFilePath();
                var backupDirectory = Path.Combine(Path.GetTempPath(), "SDWallpaperEngine", "WallpaperBackup");
                Directory.CreateDirectory(backupDirectory);

                var extension = Path.GetExtension(currentWallpaper);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".bmp";
                }

                var backupPath = Path.Combine(backupDirectory, $"original_wallpaper_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}{extension}");
                File.Copy(currentWallpaper, backupPath, overwrite: false);
                _savedWallpaperPath = backupPath;
                AddLog($"Wallpaper backup saved: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                _savedWallpaperPath = null;
                AddErrorLog($"Wallpaper backup failed: {ex.Message}");
                return false;
            }
        }

        private async void WindowMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _isClosing = true;
            checkBox_enabled.CheckedChanged -= CheckBoxEnabled_CheckedChanged;
            checkBox_record.CheckedChanged -= CheckBoxRecord_CheckedChanged;
            await StopLoopAsync().ConfigureAwait(true);
            StopGifRecording();
            _client.Dispose();
        }

        private void StartLoop()
        {
            if (_loopTask is not null && !_loopTask.IsCompleted)
            {
                return;
            }

            _loopCancellation = new CancellationTokenSource();
            _loopTask = RunLoopAsync(_loopCancellation.Token);
            UpdateStatus("running");
        }

        private async Task StopLoopAsync()
        {
            if (_loopCancellation is null)
            {
                UpdateStatus("stopped");
                return;
            }

            _loopCancellation.Cancel();

            try
            {
                if (_loopTask is not null)
                {
                    await _loopTask.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (_loopCancellation.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                UpdateStatus($"error: {ex.Message}");
            }
            finally
            {
                _generatedWallpapers.Clear();
                _loopCancellation.Dispose();
                _loopCancellation = null;
                _loopTask = null;
                UpdateStatus("stopped");
            }
        }

        private async Task RunLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cycleSettings = GetCycleSettings();
                    var cycleStopwatch = Stopwatch.StartNew();

                    if (_generatedWallpapers.Count == 0)
                    {
                        UpdateStatus("generating batch");
                        AddLog($"Generating batch... batch={cycleSettings.Batch}, steps={cycleSettings.Steps}");
                        var generatedBatch = await _automationService.GenerateBatchAsync(cycleSettings.Batch, cycleSettings.Steps, cycleSettings.Denoise, cycleSettings.MaxImagesKeep, _progress, AddLog, cancellationToken).ConfigureAwait(false);

                        if (generatedBatch.Count == 0)
                        {
                            AddErrorLog("Batch generation returned 0 images.");
                        }

                        foreach (var path in generatedBatch)
                        {
                            _generatedWallpapers.Enqueue(path);
                        }

                        AddLog($"Batch generated {generatedBatch.Count} image(s) in {cycleStopwatch.Elapsed.TotalSeconds:F1}s.");
                    }

                    if (_generatedWallpapers.Count > 0)
                    {
                        var nextWallpaper = _generatedWallpapers.Dequeue();
                        WallpaperManager.SetWallpaper(nextWallpaper);
                        UpdatePreviewImage(nextWallpaper);
                        TryRecordGifFrame(nextWallpaper, cycleSettings.GifDelayMs);
                        UpdateStatus($"showing ({_generatedWallpapers.Count} queued)");
                        AddSuccessLog($"Applied wallpaper in {cycleStopwatch.Elapsed.TotalSeconds:F1}s (queue left: {_generatedWallpapers.Count}).");
                        _consecutiveFailures = 0;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _consecutiveFailures++;
                    await TryRecoverAfterFailureAsync(cancellationToken).ConfigureAwait(false);
                    UpdateProgress(0d);
                    UpdateStatus($"error: {ex.Message}");
                    AddErrorLog($"{ex.GetType().Name}: {ex.Message}");
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var delay = GetCycleSettings().Interval;
                UpdateStatus($"waiting {delay} ms");
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        private (int Batch, int Steps, double Denoise, int Interval, int MaxImagesKeep, int GifDelayMs) GetCycleSettings()
        {
            if (InvokeRequired)
            {
                return (ValueTuple<int, int, double, int, int, int>)Invoke(new Func<(int Batch, int Steps, double Denoise, int Interval, int MaxImagesKeep, int GifDelayMs)>(GetCycleSettings));
            }

            return ((int)numericUpDown_batch.Value, (int)numericUpDown_steps.Value, (double)numericUpDown_denoise.Value, (int)numericUpDown_interval.Value, (int)numericUpDown_maxImages.Value, (int)numericUpDown_gifDelay.Value);
        }

        private void CheckBoxRecord_CheckedChanged(object? sender, EventArgs e)
        {
            if (!_suppressRecordSync)
            {
                _suppressRecordSync = true;
                saveFramesToolStripMenuItem.Checked = checkBox_record.Checked;
                _suppressRecordSync = false;
            }

            if (checkBox_record.Checked)
            {
                StartGifRecording();
            }
            else
            {
                StopGifRecording();
            }
        }

        private void StartGifRecording()
        {
            try
            {
                StopGifRecording();
                var outputDir = WallpaperManager.ResolveOutputDirectoryPath(_settings.OutputDirectory);
                Directory.CreateDirectory(outputDir);

                _gifRecordingSession = GifRecordingSession.Start(outputDir);
                AddLog($"GIF recording started: {_gifRecordingSession.OutputFilePath}");
            }
            catch (Exception ex)
            {
                AddErrorLog($"GIF recording start failed: {ex.Message}");
                checkBox_record.Checked = false;
            }
        }

        private void StopGifRecording()
        {
            if (_gifRecordingSession is null)
            {
                return;
            }

            try
            {
                var finalizedPath = _gifRecordingSession.FinalizeAndSave();
                AddLog(string.IsNullOrWhiteSpace(finalizedPath)
                    ? "GIF recording stopped (not enough frames)."
                    : $"GIF recording finalized: {finalizedPath}");
            }
            catch (Exception ex)
            {
                AddErrorLog($"GIF recording finalize failed: {ex.Message}");
            }
            finally
            {
                _gifRecordingSession.Dispose();
                _gifRecordingSession = null;
            }
        }

        private void TryRecordGifFrame(string wallpaperPath, int frameDelayMs)
        {
            try
            {
                _gifRecordingSession?.AddFrame(wallpaperPath, frameDelayMs);
            }
            catch (Exception ex)
            {
                AddErrorLog($"GIF frame add failed: {ex.Message}");
            }
        }

        private async Task TryRecoverAfterFailureAsync(CancellationToken cancellationToken)
        {
            try
            {
                var payload = _consecutiveFailures >= 3
                    ? new { unload_models = true }
                    : new { unload_models = false };

                await _client.FreeAsync(payload, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private void TryShowCurrentWallpaperPreview()
        {
            try
            {
                var currentWallpaper = WallpaperManager.ResolveCurrentWallpaperFilePath();
                UpdatePreviewImage(currentWallpaper);
                UpdateStatus("ready");
                AddLog($"Current wallpaper detected: {currentWallpaper}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"preview error: {ex.Message}");
                AddErrorLog($"Wallpaper preview failed: {ex.Message}");
            }
        }

        private void UpdateProgress(double value)
        {
            value = Math.Clamp(value, 0d, 1d);
            SetTitle($"{BaseTitle} - {value:P0}");
        }

        private void UpdateStatus(string status)
        {
            SetTitle($"{BaseTitle} - {status}");
        }

        private void UpdatePreviewImage(string imagePath)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdatePreviewImage(imagePath)));
                return;
            }

            using var stream = File.OpenRead(imagePath);
            using var image = Image.FromStream(stream);
            var nextImage = new Bitmap(image);

            var previousImage = pictureBox_preview.Image;
            pictureBox_preview.Image = nextImage;
            previousImage?.Dispose();
        }

        private void SetTitle(string title)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke(new Action(() => Text = title));
                return;
            }

            Text = title;
        }

        private void button_openOutput_Click(object sender, EventArgs e)
        {
            var outputDir = WallpaperManager.ResolveOutputDirectoryPath(_settings.OutputDirectory);
            Directory.CreateDirectory(outputDir);

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = outputDir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open output directory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSuccessLog(string message)
        {
            AddLog($"OK  {message}");
        }

        private void AddErrorLog(string message)
        {
            AddLog($"ERR {message}");
        }

        private void AddLog(string message)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            var entry = $"[{DateTime.Now:HH:mm:ss}] {message}";

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddLogCore(entry)));
                return;
            }

            AddLogCore(entry);
        }

        private void AddLogCore(string entry)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (listBox_log.Items.Count >= MaxLogEntries)
            {
                listBox_log.Items.RemoveAt(0);
            }

            listBox_log.Items.Add(entry);
            listBox_log.TopIndex = listBox_log.Items.Count - 1;
        }

        private void ListBoxLog_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                CopyAllLogsToClipboard();
                return;
            }

            var index = listBox_log.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                var value = listBox_log.Items[index]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Clipboard.SetText(value);
                }
            }
        }

        private void CopyAllLogsToClipboard()
        {
            if (listBox_log.Items.Count == 0)
            {
                return;
            }

            var builder = new StringBuilder();
            foreach (var item in listBox_log.Items)
            {
                if (item is not null)
                {
                    builder.AppendLine(item.ToString());
                }
            }

            var text = builder.ToString().TrimEnd();
            if (!string.IsNullOrWhiteSpace(text))
            {
                Clipboard.SetText(text);
            }
        }

        private async Task<bool> IsComfyUiApiReachableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(_settings.ComfyUiApiUrl, UriKind.Absolute),
                    Timeout = TimeSpan.FromSeconds(3)
                };

                if (!string.IsNullOrWhiteSpace(_settings.ComfyUiApiKey))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-API-Key", _settings.ComfyUiApiKey);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_settings.ComfyUiApiKey}");
                }

                using var response = await client.GetAsync("/system_stats", cancellationToken).ConfigureAwait(true);
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        private Process StartComfyUiProcess()
        {
            var exePath = Environment.ExpandEnvironmentVariables(_settings.ComfyUiExePath);
            if (string.IsNullOrWhiteSpace(exePath))
            {
                throw new InvalidOperationException("ComfyUI executable path is not configured.");
            }

            exePath = Path.GetFullPath(exePath);
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException("ComfyUI executable was not found.", exePath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = _settings.ComfyUiLaunchArguments ?? string.Empty,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = Process.Start(startInfo);
            return process ?? throw new InvalidOperationException("ComfyUI process could not be started.");
        }

        private int GetComfyUiProcessCount()
        {
            var exePath = Environment.ExpandEnvironmentVariables(_settings.ComfyUiExePath);
            var processName = Path.GetFileNameWithoutExtension(exePath);
            if (string.IsNullOrWhiteSpace(processName))
            {
                processName = "ComfyUI";
            }

            try
            {
                return Process.GetProcessesByName(processName).Length;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<bool> EnsureComfyUiRunningAsync()
        {
            if (await IsComfyUiApiReachableAsync().ConfigureAwait(true))
            {
                AddSuccessLog($"ComfyUI API erreichbar: {_settings.ComfyUiApiUrl}");
                return true;
            }

            var processCount = GetComfyUiProcessCount();
            if (processCount > 0)
            {
                AddErrorLog($"ComfyUI läuft bereits ({processCount} Prozess(e)), aber die API unter {_settings.ComfyUiApiUrl} ist nicht erreichbar. Wahrscheinlich ist die Adresse oder der Port falsch konfiguriert.");
                return false;
            }

            try
            {
                StartComfyUiProcess();
                AddLog($"ComfyUI gestartet: {_settings.ComfyUiExePath}");
            }
            catch (Exception ex)
            {
                AddErrorLog($"ComfyUI konnte nicht gestartet werden: {ex.Message}");
                return false;
            }

            for (var i = 0; i < 20; i++)
            {
                await Task.Delay(1000).ConfigureAwait(true);
                if (await IsComfyUiApiReachableAsync().ConfigureAwait(true))
                {
                    AddSuccessLog($"ComfyUI API ist jetzt erreichbar: {_settings.ComfyUiApiUrl}");
                    return true;
                }
            }

            processCount = GetComfyUiProcessCount();
            if (processCount > 0)
            {
                AddErrorLog($"ComfyUI wurde gestartet, aber die API unter {_settings.ComfyUiApiUrl} ist noch nicht erreichbar. Prüfe Port, Startargumente und API-Konfiguration.");
            }
            else
            {
                AddErrorLog("ComfyUI wurde gestartet, läuft aber offenbar nicht mehr.");
            }

            return false;
        }

        private async void button_reset_Click(object sender, EventArgs e)
        {
            if (checkBox_enabled.Checked)
            {
                _suppressEnabledChanged = true;
                checkBox_enabled.Checked = false;
                _suppressEnabledChanged = false;
            }

            if (_loopTask is not null && !_loopTask.IsCompleted)
            {
                try
                {
                    await StopLoopAsync().ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    AddErrorLog($"Wallpaper engine stop failed: {ex.Message}");
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(_savedWallpaperPath) || !File.Exists(_savedWallpaperPath))
            {
                AddErrorLog("No saved wallpaper available to restore.");
                return;
            }

            try
            {
                WallpaperManager.SetWallpaper(_savedWallpaperPath);
                TryShowCurrentWallpaperPreview();
                AddSuccessLog("Wallpaper restored.");
            }
            catch (Exception ex)
            {
                AddErrorLog($"Wallpaper restore failed: {ex.Message}");
            }
        }

        private async void button_comfyUi_Click(object sender, EventArgs e)
        {
            button_comfyUi.Enabled = false;
            try
            {
                await EnsureComfyUiRunningAsync().ConfigureAwait(true);
            }
            finally
            {
                button_comfyUi.Enabled = true;
            }
        }

        private void WindowMain_Load(object sender, EventArgs e)
        {
            _suppressSettingsChangeEvents = true;

            toolStripTextBox_outputFolder.Text = _settings.OutputDirectory;
            toolStripTextBox_outputDirectory.Text = _settings.OutputDirectory;
            toolStripTextBox_comfyUiApiUrl.Text = _settings.ComfyUiApiUrl;
            toolStripTextBox_comfyUiApiKey.Text = _settings.ComfyUiApiKey;
            toolStripTextBox_comfyUiExePath.Text = _settings.ComfyUiExePath;
            toolStripTextBox_comfyUiLaunchArguments.Text = _settings.ComfyUiLaunchArguments;
            toolStripTextBox_comfyUiWorkflowPath.Text = _settings.ComfyUiWorkflowTemplatePath;
            toolStripTextBox_positivePrompt.Text = _settings.PositivePrompt;
            toolStripTextBox_negativePrompt.Text = _settings.NegativePrompt;
            toolStripTextBox_maxKeepAmount.Text = _settings.MaxImagesKeep.ToString();
            toolStripTextBox_maxTimeout.Text = _settings.MaxTimeoutSeconds.ToString();
            toolStripTextBox_maxRetries.Text = _settings.MaxRetries.ToString();

            _suppressSettingsChangeEvents = false;

            _suppressRecordSync = true;
            saveFramesToolStripMenuItem.Checked = checkBox_record.Checked;
            _suppressRecordSync = false;
        }

        private void saveFramesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressRecordSync)
            {
                return;
            }

            _suppressRecordSync = true;
            checkBox_record.Checked = saveFramesToolStripMenuItem.Checked;
            _suppressRecordSync = false;
        }

        private void openOutputFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button_openOutput_Click(sender, e);
        }

        private void cloarOutputFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var outputDir = WallpaperManager.ResolveOutputDirectoryPath(_settings.OutputDirectory);
            Directory.CreateDirectory(outputDir);

            try
            {
                var deletedFiles = 0;
                foreach (var file in Directory.EnumerateFiles(outputDir, "*", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(file);
                    deletedFiles++;
                }

                foreach (var dir in Directory.EnumerateDirectories(outputDir, "*", SearchOption.TopDirectoryOnly))
                {
                    Directory.Delete(dir, recursive: true);
                }

                AddSuccessLog($"Output folder cleared: {deletedFiles} file(s) removed.");
            }
            catch (Exception ex)
            {
                AddErrorLog($"Could not clear output folder: {ex.Message}");
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddLog("Help: Use Options > Edit appsettings to update API URL, prompts, retries, timeout and output path.");
        }

        private void toolStripTextBox_outputFolder_TextChanged(object sender, EventArgs e)
        {
            ApplyOutputDirectoryFromMenuText(toolStripTextBox_outputFolder.Text, updateOutputFolderTextBox: false);
        }

        private void toolStripTextBox_outputDirectory_TextChanged(object sender, EventArgs e)
        {
            ApplyOutputDirectoryFromMenuText(toolStripTextBox_outputDirectory.Text, updateOutputFolderTextBox: true);
        }

        private void ApplyOutputDirectoryFromMenuText(string text, bool updateOutputFolderTextBox)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.OutputDirectory = text;

            _suppressSettingsChangeEvents = true;
            if (updateOutputFolderTextBox)
            {
                toolStripTextBox_outputFolder.Text = text;
            }
            else
            {
                toolStripTextBox_outputDirectory.Text = text;
            }

            _suppressSettingsChangeEvents = false;
        }

        private void toolStripTextBox_comfyUiApiUrl_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.ComfyUiApiUrl = toolStripTextBox_comfyUiApiUrl.Text.Trim();
        }

        private void toolStripTextBox_comfyUiApiKey_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.ComfyUiApiKey = toolStripTextBox_comfyUiApiKey.Text;
        }

        private void toolStripTextBox_comfyUiExePath_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.ComfyUiExePath = toolStripTextBox_comfyUiExePath.Text.Trim();
        }

        private void toolStripTextBox_comfyUiLaunchArguments_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.ComfyUiLaunchArguments = toolStripTextBox_comfyUiLaunchArguments.Text;
        }

        private void toolStripTextBox_comfyUiWorkflowPath_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.ComfyUiWorkflowTemplatePath = toolStripTextBox_comfyUiWorkflowPath.Text.Trim();
        }

        private void toolStripTextBox_positivePrompt_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.PositivePrompt = toolStripTextBox_positivePrompt.Text;
        }

        private void toolStripTextBox_negativePrompt_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            _settings.NegativePrompt = toolStripTextBox_negativePrompt.Text;
        }

        private void toolStripTextBox_maxKeepAmount_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            if (int.TryParse(toolStripTextBox_maxKeepAmount.Text.Trim(), out var value) && value > 0)
            {
                _settings.MaxImagesKeep = value;
                var clamped = Math.Clamp(value, (int)numericUpDown_maxImages.Minimum, (int)numericUpDown_maxImages.Maximum);
                numericUpDown_maxImages.Value = clamped;
            }
        }

        private void toolStripTextBox_maxTimeout_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            if (int.TryParse(toolStripTextBox_maxTimeout.Text.Trim(), out var value) && value > 0)
            {
                _settings.MaxTimeoutSeconds = value;
            }
        }

        private void toolStripTextBox_maxRetries_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsChangeEvents)
            {
                return;
            }

            if (int.TryParse(toolStripTextBox_maxRetries.Text.Trim(), out var value) && value > 0)
            {
                _settings.MaxRetries = value;
            }
        }
    }
}
