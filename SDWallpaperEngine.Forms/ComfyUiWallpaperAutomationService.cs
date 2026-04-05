using SDWallpaperEngine.ComfyUi;
using SDWallpaperEngine.Shared;
using System.Text.Json.Nodes;

namespace SDWallpaperEngine.Forms
{
    internal sealed class ComfyUiWallpaperAutomationService
    {
        private static readonly Dictionary<string, string[]> WidgetInputOrderByNodeType = new(StringComparer.OrdinalIgnoreCase)
        {
            ["CLIPTextEncode"] = ["text"],
            ["CheckpointLoaderSimple"] = ["ckpt_name"],
            ["RepeatImageBatch"] = ["amount"],
            ["KSampler"] = ["seed", "steps", "cfg", "sampler_name", "scheduler", "denoise"],
            ["LoadImage"] = ["image", "upload"],
            ["ImageScale"] = ["upscale_method", "width", "height", "crop"],
            ["ImageScaleBy"] = ["upscale_method", "scale_by"],
            ["SaveImage"] = ["filename_prefix"]
        };

        private readonly AppSettings _settings;
        private readonly ComfyUiApiClient _client;

        public ComfyUiWallpaperAutomationService(AppSettings settings, ComfyUiApiClient client)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IReadOnlyList<string>> GenerateBatchAsync(int batch, int steps, double denoise, int maxImagesKeep, IProgress<double>? progress, Action<string>? log, CancellationToken cancellationToken)
        {
            progress?.Report(0d);

            var sourceWallpaper = WallpaperManager.ResolveCurrentWallpaperFilePath();
            log?.Invoke($"Source wallpaper: {sourceWallpaper}");
            progress?.Report(0.05d);

            await using var sourceStream = File.OpenRead(sourceWallpaper);
            var upload = await _client.UploadImageAsync(sourceStream, Path.GetFileName(sourceWallpaper), contentType: GetContentType(sourceWallpaper), progress: progress, cancellationToken: cancellationToken).ConfigureAwait(false);
            var uploadFileName = GetString(upload, "name") ?? Path.GetFileName(sourceWallpaper);
            var uploadSubfolder = GetString(upload, "subfolder");
            var uploadType = GetString(upload, "type") ?? "input";

            progress?.Report(0.25d);

            var workflowTemplate = await LoadWorkflowTemplateAsync(cancellationToken).ConfigureAwait(false);
            var prompt = BuildPromptFromWorkflowTemplate(workflowTemplate);
            log?.Invoke($"Workflow loaded. Prompt nodes: {prompt.Count}");
            ApplyWorkflowParameters(prompt, uploadFileName, uploadSubfolder, uploadType, batch, steps, denoise, _settings.PositivePrompt, _settings.NegativePrompt);
            log?.Invoke(GetKSamplerDebugSummary(prompt));
            log?.Invoke($"Prompt text active: positive='{_settings.PositivePrompt}', negative='{_settings.NegativePrompt}'");

            var clientId = GetClientId();
            var submission = await _client.SubmitPromptAsync(prompt, clientId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(submission?.PromptId))
            {
                var errorText = submission?.Error?.ToJsonString() ?? "unknown";
                var nodeErrors = submission?.NodeErrors?.ToJsonString() ?? "unknown";
                throw new InvalidOperationException($"ComfyUI did not return a prompt_id. Error: {errorText}. NodeErrors: {nodeErrors}");
            }

            log?.Invoke($"Prompt submitted. prompt_id={submission.PromptId}");

            progress?.Report(0.40d);

            var history = await WaitForHistoryAsync(submission.PromptId, progress, cancellationToken).ConfigureAwait(false);
            var images = FindOutputImageReferences(history);
            if (images.Count == 0)
            {
                throw new InvalidOperationException("The workflow did not produce image outputs.");
            }

            log?.Invoke($"History received. Output images: {images.Count}");

            var generatedPaths = new List<string>(images.Count);

            for (var i = 0; i < images.Count; i++)
            {
                var image = images[i];
                var viewQuery = new Dictionary<string, string?>
                {
                    ["filename"] = image.Filename,
                    ["subfolder"] = image.Subfolder,
                    ["type"] = image.Type
                };

                var bytes = await _client.GetBytesAsync("/view", viewQuery, null, cancellationToken).ConfigureAwait(false);
                var generatedPath = WallpaperManager.CreateWallpaperCompatibleCopy(bytes, _settings.OutputDirectory, maxImagesKeep);
                generatedPaths.Add(generatedPath);
                log?.Invoke($"Saved output: {generatedPath}");

                var ratio = images.Count == 0 ? 1d : (double)(i + 1) / images.Count;
                progress?.Report(0.90d + (0.10d * ratio));
            }

            progress?.Report(1d);
            return generatedPaths;
        }

        private static JsonObject BuildPromptFromWorkflowTemplate(JsonObject workflowTemplate)
        {
            if (workflowTemplate.TryGetPropertyValue("nodes", out var nodesNode) && nodesNode is JsonArray nodes &&
                workflowTemplate.TryGetPropertyValue("links", out var linksNode) && linksNode is JsonArray links)
            {
                return ConvertGraphWorkflowToPrompt(nodes, links);
            }

            return workflowTemplate;
        }

        private static JsonObject ConvertGraphWorkflowToPrompt(JsonArray nodes, JsonArray links)
        {
            var linkMap = new Dictionary<int, (int sourceNodeId, int sourceOutputIndex)>();

            foreach (var linkNode in links)
            {
                if (linkNode is not JsonArray link || link.Count < 3)
                {
                    continue;
                }

                var linkId = GetInt(link[0]);
                var sourceNodeId = GetInt(link[1]);
                var sourceOutputIndex = GetInt(link[2]);
                if (linkId is null || sourceNodeId is null || sourceOutputIndex is null)
                {
                    continue;
                }

                linkMap[linkId.Value] = (sourceNodeId.Value, sourceOutputIndex.Value);
            }

            var prompt = new JsonObject();

            foreach (var node in nodes.OfType<JsonObject>())
            {
                var nodeId = GetInt(node["id"]);
                var classType = GetString(node, "type");
                if (nodeId is null || string.IsNullOrWhiteSpace(classType))
                {
                    continue;
                }

                var inputs = new JsonObject();
                var nodeInputs = node["inputs"] as JsonArray;
                var widgets = node["widgets_values"] as JsonArray;

                if (nodeInputs is not null)
                {
                    foreach (var nodeInput in nodeInputs.OfType<JsonObject>())
                    {
                        var inputName = GetString(nodeInput, "name");
                        if (string.IsNullOrWhiteSpace(inputName))
                        {
                            continue;
                        }

                        var linkId = GetInt(nodeInput["link"]);
                        if (linkId is not null && linkMap.TryGetValue(linkId.Value, out var linkInfo))
                        {
                            inputs[inputName] = new JsonArray(linkInfo.sourceNodeId.ToString(), linkInfo.sourceOutputIndex);
                            continue;
                        }

                    }
                }

                ApplyWidgetValues(classType, widgets, inputs);

                prompt[nodeId.Value.ToString()] = new JsonObject
                {
                    ["class_type"] = classType,
                    ["inputs"] = inputs
                };
            }

            return prompt;
        }

        private static void ApplyWidgetValues(string classType, JsonArray? widgets, JsonObject inputs)
        {
            if (widgets is null || widgets.Count == 0)
            {
                return;
            }

            if (classType.Equals("KSampler", StringComparison.OrdinalIgnoreCase))
            {
                ApplyKSamplerWidgetValues(widgets, inputs);
                return;
            }

            if (WidgetInputOrderByNodeType.TryGetValue(classType, out var widgetNames))
            {
                for (var index = 0; index < widgetNames.Length && index < widgets.Count; index++)
                {
                    var key = widgetNames[index];
                    inputs[key] = widgets[index]?.DeepClone();
                }
            }
        }

        private static void ApplyKSamplerWidgetValues(JsonArray widgets, JsonObject inputs)
        {
            if (widgets.Count == 0)
            {
                return;
            }

            inputs["seed"] = widgets[0]?.DeepClone();

            // ComfyUI graph JSON can include an extra "control after generate" widget value
            // between seed and steps. We always map the last 5 values to
            // steps/cfg/sampler_name/scheduler/denoise.
            var tailStart = Math.Max(1, widgets.Count - 5);

            if (tailStart < widgets.Count)
            {
                inputs["steps"] = widgets[tailStart]?.DeepClone();
            }

            if (tailStart + 1 < widgets.Count)
            {
                inputs["cfg"] = widgets[tailStart + 1]?.DeepClone();
            }

            if (tailStart + 2 < widgets.Count)
            {
                inputs["sampler_name"] = widgets[tailStart + 2]?.DeepClone();
            }

            if (tailStart + 3 < widgets.Count)
            {
                inputs["scheduler"] = widgets[tailStart + 3]?.DeepClone();
            }

            if (tailStart + 4 < widgets.Count)
            {
                inputs["denoise"] = widgets[tailStart + 4]?.DeepClone();
            }
        }

        private async Task<JsonObject> LoadWorkflowTemplateAsync(CancellationToken cancellationToken)
        {
            var workflowPath = ResolveWorkflowTemplatePath();
            var json = await File.ReadAllTextAsync(workflowPath, cancellationToken).ConfigureAwait(false);
            return JsonNode.Parse(json)?.AsObject() ?? throw new InvalidOperationException($"Workflow template '{workflowPath}' is not valid JSON.");
        }

        private string ResolveWorkflowTemplatePath()
        {
            var path = _settings.ComfyUiWorkflowTemplatePath;
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(AppContext.BaseDirectory, path);
        }

        private async Task<JsonNode?> WaitForHistoryAsync(string promptId, IProgress<double>? progress, CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromMinutes(15);
            var started = DateTimeOffset.UtcNow;

            while (DateTimeOffset.UtcNow - started < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                JsonNode? history;
                try
                {
                    history = await _client.GetHistoryAsync(promptId, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    history = null;
                }

                if (TryHasGeneratedImage(history))
                {
                    return history;
                }

                var elapsedRatio = (DateTimeOffset.UtcNow - started).TotalMilliseconds / timeout.TotalMilliseconds;
                progress?.Report(0.40d + Math.Min(0.48d, elapsedRatio * 0.48d));
                await Task.Delay(750, cancellationToken).ConfigureAwait(false);
            }

            throw new TimeoutException("The ComfyUI workflow did not finish in time.");
        }

        private static bool TryHasGeneratedImage(JsonNode? history)
        {
            return FindOutputImageReferences(history).Count > 0;
        }

        private static List<ComfyUiImageReference> FindOutputImageReferences(JsonNode? history)
        {
            var outputs = FindOutputsNode(history);
            var found = new List<ComfyUiImageReference>();
            CollectImageReferences(outputs, found);

            return found
                .Where(x => !string.IsNullOrWhiteSpace(x.Filename))
                .DistinctBy(x => $"{x.Type}|{x.Subfolder}|{x.Filename}")
                .ToList();
        }

        private static JsonNode? FindOutputsNode(JsonNode? node)
        {
            if (node is JsonObject obj)
            {
                if (obj.TryGetPropertyValue("outputs", out var outputs) && outputs is not null)
                {
                    return outputs;
                }

                foreach (var child in obj)
                {
                    var outputsNode = FindOutputsNode(child.Value);
                    if (outputsNode is not null)
                    {
                        return outputsNode;
                    }
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var child in array)
                {
                    var outputsNode = FindOutputsNode(child);
                    if (outputsNode is not null)
                    {
                        return outputsNode;
                    }
                }
            }

            return null;
        }

        private static void ApplyWorkflowParameters(JsonObject workflow, string uploadedFileName, string? uploadedSubfolder, string? uploadedType, int batch, int steps, double denoise, string? positivePrompt, string? negativePrompt)
        {
            var clipTextIndex = 0;

            foreach (var node in workflow.Select(node => node.Value).OfType<JsonObject>())
            {
                var classType = GetString(node, "class_type");
                if (string.IsNullOrWhiteSpace(classType))
                {
                    continue;
                }

                var inputs = node["inputs"] as JsonObject;
                if (inputs is null)
                {
                    continue;
                }

                if (classType.Equals("LoadImage", StringComparison.OrdinalIgnoreCase) || classType.Contains("LoadImage", StringComparison.OrdinalIgnoreCase))
                {
                    SetString(inputs, uploadedFileName, "image", "filename", "path");
                    if (!string.IsNullOrWhiteSpace(uploadedSubfolder))
                    {
                        SetString(inputs, uploadedSubfolder, "subfolder");
                    }

                    if (!string.IsNullOrWhiteSpace(uploadedType))
                    {
                        SetString(inputs, uploadedType, "type");
                    }
                }

                if (classType.Contains("RepeatImageBatch", StringComparison.OrdinalIgnoreCase) || classType.Contains("Batch", StringComparison.OrdinalIgnoreCase))
                {
                    SetNumber(inputs, batch, "amount", "batch_size", "count");
                }

                if (classType.Equals("KSampler", StringComparison.OrdinalIgnoreCase))
                {
                    SetNumber(inputs, steps, "steps");
                    SetNumber(inputs, batch, "batch_size");
                    SetDouble(inputs, denoise, "denoise");
                }

                if (classType.Equals("CLIPTextEncode", StringComparison.OrdinalIgnoreCase))
                {
                    if (clipTextIndex == 0 && !string.IsNullOrWhiteSpace(positivePrompt))
                    {
                        SetString(inputs, positivePrompt, "text");
                    }
                    else if (clipTextIndex == 1 && !string.IsNullOrWhiteSpace(negativePrompt))
                    {
                        SetString(inputs, negativePrompt, "text");
                    }

                    clipTextIndex++;
                }
            }
        }

        private static void SetString(JsonObject inputs, string value, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (inputs.ContainsKey(key))
                {
                    inputs[key] = value;
                }
            }
        }

        private static void SetNumber(JsonObject inputs, int value, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (inputs.ContainsKey(key))
                {
                    inputs[key] = value;
                }
            }
        }

        private static void SetDouble(JsonObject inputs, double value, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (inputs.ContainsKey(key))
                {
                    inputs[key] = value;
                }
            }
        }

        private static int? GetInt(JsonNode? node)
        {
            if (node is null)
            {
                return null;
            }

            if (node is JsonValue value)
            {
                if (value.TryGetValue<int>(out var intValue))
                {
                    return intValue;
                }

                if (value.TryGetValue<string>(out var stringValue) && int.TryParse(stringValue, out var parsedValue))
                {
                    return parsedValue;
                }
            }

            return null;
        }

        private static string? GetString(JsonNode? node, string propertyName)
        {
            return node?[propertyName]?.GetValue<string>();
        }

        private static string? GetContentType(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private static string GetClientId()
        {
            return $"SDWallpaperEngine-{Environment.MachineName}-{Environment.ProcessId}";
        }

        private static string GetKSamplerDebugSummary(JsonObject prompt)
        {
            foreach (var promptNode in prompt)
            {
                if (promptNode.Value is not JsonObject node)
                {
                    continue;
                }

                var classType = GetString(node, "class_type");
                if (!string.Equals(classType, "KSampler", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (node["inputs"] is not JsonObject inputs)
                {
                    return "KSampler mapping: missing inputs";
                }

                return $"KSampler mapping: steps={inputs["steps"]}, cfg={inputs["cfg"]}, sampler={inputs["sampler_name"]}, scheduler={inputs["scheduler"]}, denoise={inputs["denoise"]}";
            }

            return "KSampler mapping: node not found";
        }

        private static void CollectImageReferences(JsonNode? node, List<ComfyUiImageReference> collector)
        {
            if (node is null)
            {
                return;
            }

            if (node is JsonObject obj)
            {
                if (obj.TryGetPropertyValue("filename", out var filenameNode) && filenameNode is not null)
                {
                    collector.Add(new ComfyUiImageReference(
                        filenameNode.GetValue<string>(),
                        obj["subfolder"]?.GetValue<string>(),
                        obj["type"]?.GetValue<string>()));
                }

                foreach (var child in obj)
                {
                    CollectImageReferences(child.Value, collector);
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    CollectImageReferences(item, collector);
                }
            }
        }

        private sealed record ComfyUiImageReference(string Filename, string? Subfolder, string? Type);
    }
}
