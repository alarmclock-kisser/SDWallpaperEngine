using SDWallpaperEngine.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SDWallpaperEngine.ComfyUi
{
    public sealed class ComfyUiApiClient : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly AppSettings _settings;
        private readonly HttpClient _client;
        private readonly bool _ownsClient;

        public ComfyUiApiClient(AppSettings settings)
            : this(settings, null)
        {
        }

        public ComfyUiApiClient(AppSettings settings, HttpClient? httpClient)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _client = httpClient ?? new HttpClient();
            _ownsClient = httpClient is null;

            if (_client.BaseAddress is null)
            {
                _client.BaseAddress = new Uri(_settings.ComfyUiApiUrl, UriKind.Absolute);
            }

            _client.Timeout = TimeSpan.FromSeconds(_settings.MaxTimeoutSeconds);

            if (!string.IsNullOrWhiteSpace(_settings.ComfyUiApiKey))
            {
                if (!_client.DefaultRequestHeaders.Contains("X-API-Key"))
                {
                    _client.DefaultRequestHeaders.Add("X-API-Key", _settings.ComfyUiApiKey);
                }

                if (_client.DefaultRequestHeaders.Authorization is null)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ComfyUiApiKey);
                }
            }
        }

        public Uri BaseUri => _client.BaseAddress ?? new Uri(_settings.ComfyUiApiUrl, UriKind.Absolute);

        public Uri GetWebSocketUri(IDictionary<string, string?>? query = null)
        {
            var builder = new UriBuilder(BaseUri)
            {
                Scheme = BaseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws",
                Path = "ws",
                Query = BuildQueryString(query)
            };

            return builder.Uri;
        }

        public ComfyUiWebSocketClient CreateWebSocketClient(IDictionary<string, string?>? query = null)
        {
            return new ComfyUiWebSocketClient(GetWebSocketUri(query), _settings.ComfyUiApiKey);
        }

        public ComfyUiWebSocketClient CreateWebSocketClient(string? clientId)
        {
            var query = string.IsNullOrWhiteSpace(clientId)
                ? null
                : new Dictionary<string, string?> { ["clientId"] = clientId };

            return CreateWebSocketClient(query);
        }

        public Task<string> GetHomePageAsync(CancellationToken cancellationToken = default)
        {
            return GetStringAsync("/", cancellationToken);
        }

        public Task<JsonNode?> GetEmbeddingsAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/embeddings", cancellationToken);
        }

        public Task<JsonNode?> GetExtensionsAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/extensions", cancellationToken);
        }

        public Task<JsonNode?> GetFeaturesAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/features", cancellationToken);
        }

        public Task<JsonNode?> GetModelsAsync(string? folder = null, CancellationToken cancellationToken = default)
        {
            return string.IsNullOrWhiteSpace(folder)
                ? GetJsonNodeAsync("/models", cancellationToken)
                : GetJsonNodeAsync($"/models/{EncodePath(folder)}", cancellationToken);
        }

        public Task<JsonNode?> GetWorkflowTemplatesAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/workflow_templates", cancellationToken);
        }

        public Task<ComfyUiSystemStats?> GetSystemStatsAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonAsync<ComfyUiSystemStats>("/system_stats", cancellationToken);
        }

        public Task<JsonNode?> GetPromptAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/prompt", cancellationToken);
        }

        public Task<ComfyUiPromptSubmissionResult?> SubmitPromptAsync(object prompt, string? clientId = null, object? extraData = null, CancellationToken cancellationToken = default)
        {
            var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["prompt"] = prompt
            };

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                payload["client_id"] = clientId;
            }

            if (extraData is not null)
            {
                payload["extra_data"] = extraData;
            }

            return PostJsonAsync<ComfyUiPromptSubmissionResult>("/prompt", payload, cancellationToken);
        }

        public Task<JsonNode?> GetObjectInfoAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/object_info", cancellationToken);
        }

        public Task<JsonNode?> GetObjectInfoAsync(string nodeClass, CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync($"/object_info/{EncodePath(nodeClass)}", cancellationToken);
        }

        public Task<JsonNode?> GetHistoryAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/history", cancellationToken);
        }

        public Task<JsonNode?> GetHistoryAsync(string promptId, CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync($"/history/{EncodePath(promptId)}", cancellationToken);
        }

        public Task<JsonNode?> PostHistoryAsync(object? payload = null, CancellationToken cancellationToken = default)
        {
            return PostJsonAsync("/history", payload, cancellationToken);
        }

        public Task<ComfyUiQueueState?> GetQueueAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonAsync<ComfyUiQueueState>("/queue", cancellationToken);
        }

        public Task<JsonNode?> PostQueueAsync(object? payload = null, CancellationToken cancellationToken = default)
        {
            return PostJsonAsync("/queue", payload, cancellationToken);
        }

        public Task<JsonNode?> InterruptAsync(CancellationToken cancellationToken = default)
        {
            return PostJsonAsync("/interrupt", null, cancellationToken);
        }

        public Task<JsonNode?> FreeAsync(object? payload = null, CancellationToken cancellationToken = default)
        {
            return PostJsonAsync("/free", payload, cancellationToken);
        }

        public Task<JsonNode?> GetUserDataAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/userdata", query, cancellationToken);
        }

        public Task<JsonNode?> GetUserDataV2Async(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/v2/userdata", query, cancellationToken);
        }

        public Task<byte[]> GetUserDataFileAsync(string filePath, IDictionary<string, string?>? query = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return GetBytesAsync($"/userdata/{EncodePath(filePath)}", query, progress, cancellationToken);
        }

        public Task<JsonNode?> UploadUserDataFileAsync(string filePath, Stream fileContent, string fileName, string? contentType = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return UploadFileAsync($"/userdata/{EncodePath(filePath)}", fileContent, fileName, contentType, progress, cancellationToken);
        }

        public Task<JsonNode?> DeleteUserDataFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return DeleteJsonAsync($"/userdata/{EncodePath(filePath)}", cancellationToken);
        }

        public Task<JsonNode?> MoveUserDataFileAsync(string filePath, string destinationPath, CancellationToken cancellationToken = default)
        {
            return PostJsonAsync($"/userdata/{EncodePath(filePath)}/move/{EncodePath(destinationPath)}", null, cancellationToken);
        }

        public Task<JsonNode?> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/users", cancellationToken);
        }

        public Task<JsonNode?> CreateUserAsync(object? payload = null, CancellationToken cancellationToken = default)
        {
            return PostJsonAsync("/users", payload, cancellationToken);
        }

        public Task<byte[]> GetViewAsync(IDictionary<string, string?>? query = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return GetBytesAsync("/view", query, progress, cancellationToken);
        }

        public Task<JsonNode?> GetViewMetadataAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync("/view_metadata/", query, cancellationToken);
        }

        public Task<JsonNode?> UploadImageAsync(Stream imageContent, string fileName, string? contentType = null, string? subfolder = null, string? type = null, bool overwrite = false, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return UploadFileCoreAsync("/upload/image", imageContent, fileName, contentType, progress, cancellationToken, subfolder, type, overwrite);
        }

        public Task<JsonNode?> UploadMaskAsync(Stream maskContent, string fileName, string? contentType = null, string? subfolder = null, string? type = null, bool overwrite = false, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return UploadFileCoreAsync("/upload/mask", maskContent, fileName, contentType, progress, cancellationToken, subfolder, type, overwrite);
        }

        public Task<JsonNode?> GetJsonNodeAsync(string path, CancellationToken cancellationToken = default)
        {
            return GetJsonNodeAsync(path, null, cancellationToken);
        }

        public Task<JsonNode?> GetJsonNodeAsync(string path, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            return SendJsonCoreAsync<JsonNode?>(HttpMethod.Get, path, null, query, cancellationToken);
        }

        public Task<T?> GetJsonAsync<T>(string path, CancellationToken cancellationToken = default)
        {
            return GetJsonAsync<T>(path, null, cancellationToken);
        }

        public async Task<T?> GetJsonAsync<T>(string path, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(() => CreateRequest(HttpMethod.Get, path, null, query), cancellationToken).ConfigureAwait(false);
            return await DeserializeAsync<T>(response, cancellationToken).ConfigureAwait(false);
        }

        public Task<T?> PostJsonAsync<T>(string path, object? payload = null, CancellationToken cancellationToken = default)
        {
            return PostJsonAsync<T>(path, payload, null, cancellationToken);
        }

        public async Task<T?> PostJsonAsync<T>(string path, object? payload, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(() => CreateRequest(HttpMethod.Post, path, payload, query), cancellationToken).ConfigureAwait(false);
            return await DeserializeAsync<T>(response, cancellationToken).ConfigureAwait(false);
        }

        public Task<JsonNode?> PostJsonAsync(string path, object? payload = null, CancellationToken cancellationToken = default)
        {
            return SendJsonCoreAsync<JsonNode?>(HttpMethod.Post, path, payload, null, cancellationToken);
        }

        public Task<JsonNode?> DeleteJsonAsync(string path, CancellationToken cancellationToken = default)
        {
            return DeleteJsonAsync(path, null, cancellationToken);
        }

        public async Task<JsonNode?> DeleteJsonAsync(string path, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(() => CreateRequest(HttpMethod.Delete, path, null, query), cancellationToken).ConfigureAwait(false);
            return await DeserializeAsync<JsonNode?>(response, cancellationToken).ConfigureAwait(false);
        }

        public Task<string> GetStringAsync(string path, CancellationToken cancellationToken = default)
        {
            return GetStringAsync(path, null, cancellationToken);
        }

        public async Task<string> GetStringAsync(string path, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(() => CreateRequest(HttpMethod.Get, path, null, query), cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<byte[]> GetBytesAsync(string path, IDictionary<string, string?>? query = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return GetBytesCoreAsync(path, query, progress, cancellationToken);
        }

        public async Task<byte[]> GetBytesCoreAsync(string path, IDictionary<string, string?>? query, IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            progress?.Report(0d);
            using var response = await SendAsync(() => CreateRequest(HttpMethod.Get, path, null, query), cancellationToken).ConfigureAwait(false);
            return await ReadResponseBytesAsync(response.Content, progress, cancellationToken).ConfigureAwait(false);
        }

        public Task<JsonNode?> UploadFileAsync(string path, Stream fileContent, string fileName, string? contentType = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default, string? subfolder = null, string? type = null, bool overwrite = false)
        {
            return UploadFileCoreAsync(path, fileContent, fileName, contentType, progress, cancellationToken, subfolder, type, overwrite);
        }

        public async Task<JsonNode?> UploadFileCoreAsync(string path, Stream fileContent, string fileName, string? contentType, IProgress<double>? progress, CancellationToken cancellationToken, string? subfolder, string? type, bool overwrite)
        {
            progress?.Report(0d);
            using var response = await SendAsync(() => CreateMultipartRequest(path, fileContent, fileName, contentType, progress, subfolder, type, overwrite), cancellationToken).ConfigureAwait(false);
            return await DeserializeAsync<JsonNode?>(response, cancellationToken).ConfigureAwait(false);
        }

        public Task<T?> SendJsonAsync<T>(HttpMethod method, string path, object? payload = null, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
        {
            return SendJsonCoreAsync<T>(method, path, payload, query, cancellationToken);
        }

        private static async Task<byte[]> ReadResponseBytesAsync(HttpContent content, IProgress<double>? progress, CancellationToken cancellationToken)
        {
            await using var responseStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var memory = new MemoryStream();

            var buffer = new byte[81920];
            var totalLength = content.Headers.ContentLength;
            long totalRead = 0;
            int read;

            while ((read = await responseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await memory.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                totalRead += read;

                if (totalLength is > 0)
                {
                    progress?.Report(Math.Min(0.99d, (double)totalRead / totalLength.Value));
                }
            }

            progress?.Report(1d);
            return memory.ToArray();
        }

        public async Task<T?> SendJsonCoreAsync<T>(HttpMethod method, string path, object? payload, IDictionary<string, string?>? query, CancellationToken cancellationToken)
        {
            using var response = await SendAsync(() => CreateRequest(method, path, payload, query), cancellationToken).ConfigureAwait(false);
            return await DeserializeAsync<T>(response, cancellationToken).ConfigureAwait(false);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? payload, IDictionary<string, string?>? query)
        {
            var request = new HttpRequestMessage(method, CreateUri(path, query));

            if (payload is not null)
            {
                request.Content = JsonContent.Create(payload, options: JsonOptions);
            }

            return request;
        }

        private HttpRequestMessage CreateMultipartRequest(string path, Stream fileContent, string fileName, string? contentType, IProgress<double>? progress, string? subfolder, string? type, bool overwrite)
        {
            if (fileContent.CanSeek)
            {
                fileContent.Position = 0;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, CreateUri(path));
            var form = new MultipartFormDataContent();

            var file = new ProgressStreamContent(fileContent, progress, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            form.Add(file, "image", fileName);

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                form.Add(new StringContent(subfolder, Encoding.UTF8), "subfolder");
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                form.Add(new StringContent(type, Encoding.UTF8), "type");
            }

            form.Add(new StringContent(overwrite.ToString().ToLowerInvariant(), Encoding.UTF8), "overwrite");

            request.Content = form;
            return request;
        }

        private Uri CreateUri(string path, IDictionary<string, string?>? query = null)
        {
            var normalizedPath = string.IsNullOrWhiteSpace(path)
                ? "/"
                : path.StartsWith('/') ? path : $"/{path}";

            var builder = new UriBuilder(new Uri(BaseUri, normalizedPath))
            {
                Query = BuildQueryString(query)
            };

            return builder.Uri;
        }

        private static string BuildQueryString(IDictionary<string, string?>? query)
        {
            if (query is null || query.Count == 0)
            {
                return string.Empty;
            }

            var parts = new List<string>(query.Count);

            foreach (var pair in query)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    continue;
                }

                parts.Add($"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}");
            }

            return string.Join("&", parts);
        }

        private static string EncodePath(string path)
        {
            var segments = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("/", segments.Select(Uri.EscapeDataString));
        }

        private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
        {
            var attempts = Math.Max(1, _settings.MaxRetries);
            Exception? lastException = null;

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                using var request = requestFactory();

                try
                {
                    var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                    if ((int)response.StatusCode >= 500 && attempt < attempts)
                    {
                        response.Dispose();
                        await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        var message = string.IsNullOrWhiteSpace(errorBody)
                            ? $"The ComfyUI request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase})."
                            : $"The ComfyUI request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}). Response: {errorBody}";

                        throw new HttpRequestException(message, null, response.StatusCode);
                    }

                    return response;
                }
                catch (Exception ex) when (attempt < attempts && IsRetryable(ex, cancellationToken))
                {
                    lastException = ex;
                    await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken).ConfigureAwait(false);
                }
            }

            throw lastException ?? new HttpRequestException("The ComfyUI request failed.");
        }

        private static bool IsRetryable(Exception exception, CancellationToken cancellationToken)
        {
            return exception is HttpRequestException || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);
        }

        private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            if (typeof(T) == typeof(string))
            {
                var text = await new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true).ReadToEndAsync().ConfigureAwait(false);
                return (T)(object)text;
            }

            var value = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
            return value;
        }

        public void Dispose()
        {
            if (_ownsClient)
            {
                _client.Dispose();
            }
        }

        private sealed class ProgressStreamContent : HttpContent
        {
            private readonly Stream _source;
            private readonly IProgress<double>? _progress;

            public ProgressStreamContent(Stream source, IProgress<double>? progress, string contentType)
            {
                _source = source;
                _progress = progress;
                Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                if (_source.CanSeek)
                {
                    _source.Position = 0;
                }

                var buffer = new byte[81920];
                long totalRead = 0;
                long totalLength = _source.CanSeek ? _source.Length : -1;
                int read;

                while ((read = await _source.ReadAsync(buffer).ConfigureAwait(false)) > 0)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                    totalRead += read;

                    if (totalLength > 0)
                    {
                        _progress?.Report(Math.Min(0.99d, (double)totalRead / totalLength));
                    }
                }

                _progress?.Report(1d);
            }

            protected override bool TryComputeLength(out long length)
            {
                if (_source.CanSeek)
                {
                    length = _source.Length;
                    return true;
                }

                length = 0;
                return false;
            }
        }
    }

}
