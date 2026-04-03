using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace SDWallpaperEngine.ComfyUi
{
    public sealed class ComfyUiWebSocketClient : IAsyncDisposable
    {
        private readonly ClientWebSocket _client;
        private readonly Uri _webSocketUri;

        public ComfyUiWebSocketClient(Uri webSocketUri, string? apiKey = null, IDictionary<string, string?>? headers = null)
        {
            _webSocketUri = webSocketUri ?? throw new ArgumentNullException(nameof(webSocketUri));
            _client = new ClientWebSocket();

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _client.Options.SetRequestHeader("X-API-Key", apiKey);
                _client.Options.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }

            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    if (!string.IsNullOrWhiteSpace(header.Value))
                    {
                        _client.Options.SetRequestHeader(header.Key, header.Value);
                    }
                }
            }
        }

        public WebSocketState State => _client.State;

        public bool IsConnected => _client.State == WebSocketState.Open;

        public Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            return _client.ConnectAsync(_webSocketUri, cancellationToken);
        }

        public async IAsyncEnumerable<ComfyUiWebSocketMessage> ReceiveMessagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (_client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var text = await ReceiveTextAsync(cancellationToken).ConfigureAwait(false);
                if (text is null)
                {
                    yield break;
                }

                yield return ComfyUiWebSocketMessage.Parse(text);
            }
        }

        public Task SendTextAsync(string message, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            return _client.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
        }

        public async Task CloseAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string? statusDescription = null, CancellationToken cancellationToken = default)
        {
            if (_client.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await _client.CloseAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
            }
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                if (_client.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    _client.Abort();
                }
            }
            finally
            {
                _client.Dispose();
            }

            return ValueTask.CompletedTask;
        }

        private async Task<string?> ReceiveTextAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var memory = new MemoryStream();

            while (true)
            {
                var result = await _client.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                    return null;
                }

                memory.Write(buffer, 0, result.Count);

                if (result.EndOfMessage)
                {
                    break;
                }
            }

            return Encoding.UTF8.GetString(memory.ToArray());
        }
    }
}
