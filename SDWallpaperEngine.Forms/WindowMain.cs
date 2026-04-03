using SDWallpaperEngine.ComfyUi;
using SDWallpaperEngine.Shared;
using System.Collections.Generic;
using System.Diagnostics;
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
            checkBox_enabled.CheckedChanged += CheckBoxEnabled_CheckedChanged;
            checkBox_record.CheckedChanged += CheckBoxRecord_CheckedChanged;
            listBox_log.MouseDoubleClick += ListBoxLog_MouseDoubleClick;
            Shown += WindowMain_Shown;
            FormClosing += WindowMain_FormClosing;
        }

        private void WindowMain_Shown(object? sender, EventArgs e)
        {
            var outputDirectory = WallpaperManager.ResolveOutputDirectoryPath(_settings.OutputDirectory);
            Directory.CreateDirectory(outputDirectory);
            AddLog($"Output directory: {outputDirectory}");

            TryShowCurrentWallpaperPreview();

            if (checkBox_enabled.Checked)
            {
                StartLoop();
            }
        }

        private async void CheckBoxEnabled_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isClosing)
            {
                return;
            }

            if (checkBox_enabled.Checked)
            {
                StartLoop();
            }
            else
            {
                await StopLoopAsync().ConfigureAwait(true);
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



    }
}
