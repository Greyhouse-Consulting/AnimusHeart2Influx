using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnimusHeart2Influx.Animus;
using Serilog;

namespace AnimusHeart2Influx
{
    public interface IAnimusWebSocketHandler
    {
        Task Connect(CancellationToken stoppingToken = default);
        WebSocketState State { get; }
        Task Send(string message, CancellationToken stoppingToken = default);
        event OnNewMessageEventHandler OnMessage;
        event EventHandler OnClose;
        event EventHandler OnConnected;
        event EventHandler OnAuthenticated;
        Task StopReceiving(CancellationToken stoppingToken = default);
        void BeginReceiving();
    }

    
    public delegate void OnNewMessageEventHandler(object? sender, OnNewMessageEventArgs e);

    public class OnNewMessageEventArgs : EventArgs
    {
        public string Message { get; }

        public OnNewMessageEventArgs(string message)
        {
            Message = message;
        }
    }

    public class OnCloseEventArgs : EventArgs
    {

    }

    public class OnConnectEventArgs : EventArgs
    {

    }

    public class OnAuthenticatedEventArgs : EventArgs
    {

    }

    public class AnimusWebSocketHandler : IAnimusWebSocketHandler
    {
        private readonly ILogger _logger = Log.ForContext<AnimusWebSocketHandler>();
        private readonly AnimusConfiguration _configuration;
        private ClientWebSocket _client;
        private CancellationTokenSource _cancelSource;
        private Task _receiveTask;
        public event OnNewMessageEventHandler OnMessage;
        public event EventHandler OnClose;
        public event EventHandler OnConnected;
        public event EventHandler OnAuthenticated;

        public WebSocketState State => _client.State;

        public async  Task<(WebSocketReceiveResult, byte[])>  Receive(ClientWebSocket clientWebSocket, CancellationToken stoppingToken)
        {
            var buffer = new byte[1024 * 4];
            var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
            return (result, buffer);
        }

        public async Task Send(string message, CancellationToken stoppingToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);
        }

        public AnimusWebSocketHandler(AnimusConfiguration configuration)
        {
            _configuration = configuration;
            _client = new ClientWebSocket();
            _client.Options.AddSubProtocol("AHauth");

        }

        public async Task Connect(CancellationToken stoppingToken = default)
        {
            try
            {
                var auth = $"Authorization: Bearer {_configuration.Key}";

                var uri = new Uri($"{_configuration.Url}{AnimusEndpoints.WebSocket}");

                await _client.ConnectAsync(uri, CancellationToken.None);
                OnConnected?.Invoke(this, new OnConnectEventArgs());

                var bytes = Encoding.UTF8.GetBytes(auth);
                await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);

                var (result, buffer) = await Receive(_client, stoppingToken);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message.Equals("authenticated", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.Information($"Laputa says: '{ message}'");
                        OnAuthenticated?.Invoke(this, new OnAuthenticatedEventArgs());
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex,"Failed to connect to animus");
            }
        }

        public void BeginReceiving()
        {
            _cancelSource = new CancellationTokenSource();
            _receiveTask = Task.Run(async () =>
            {
                while (!_cancelSource.IsCancellationRequested)
                {
                    var (result, buffer) = await Receive(_client, _cancelSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        OnMessage?.Invoke(this, new OnNewMessageEventArgs(Encoding.UTF8.GetString(buffer, 0, result.Count)) );
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }
                }

            }, _cancelSource.Token);
        }

        public async Task  StopReceiving(CancellationToken stoppingToken = default)
        {
            _cancelSource.Cancel();
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", stoppingToken);
            _client.Abort();
            _client = new ClientWebSocket();
            _client.Options.AddSubProtocol("AHauth");
            OnClose?.Invoke(this, new OnCloseEventArgs());
        }
    }
}