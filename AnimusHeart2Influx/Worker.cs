using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using AnimusHeart2Influx.Animus;
using AnimusHeart2Influx.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Serilog;
//using WebSocketSharp;

namespace AnimusHeart2Influx
{
    public class Worker : BackgroundService
    {
        private readonly IAnimusWebSocketEventHandler _animusWebSocketEventHandler;
        private readonly MessageCounter _messageCounter;
        private readonly SlotCounter _slotCounter;
        private readonly IAnimusWebSocketHandler _webSocketHandler;
        private readonly ILogger _logger = Log.ForContext<Worker>();

        private DateTime _nextPing;

        public Worker(IAnimusWebSocketEventHandler animusWebSocketEventHandler, MessageCounter messageCounter,
            SlotCounter slotCounter, IAnimusWebSocketHandler webSocketHandler)
        {
            _animusWebSocketEventHandler = animusWebSocketEventHandler;
            _messageCounter = messageCounter;
            _slotCounter = slotCounter;
            _webSocketHandler = webSocketHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetNextPingTime();

            _messageCounter.Reset();


            var p = CreateRetryPolicy();

            //await CreateAndConfigureWebSocket();

            await _animusWebSocketEventHandler.RefreshDevices();
            //p.Execute(() => ReConnect(_ws));

            _messageCounter.Reset();
            _slotCounter.CalculateNextSlotStart();

            _webSocketHandler.OnMessage += (sender, args) =>
            {
                _animusWebSocketEventHandler.Handle(args.Message);
                _messageCounter.Tick();
                
                if (_messageCounter.HasReachSlotLimit())
                {
                    _logger.Information("Has reached slot message limit of {messageLimit}. Suspending connection", _messageCounter.MaxMessagesPerSampleSlot);
                    _webSocketHandler.StopReceiving(stoppingToken);

                    _slotCounter.CalculateNextSlotStart();
                    _logger.Information("Will resume collection at {resumeTime}", _slotCounter.NextSlotTimeStart);

                    _messageCounter.Reset();
                }
            };

            _webSocketHandler.OnClose += (sender, args) => { _logger.Information("Socket closed"); };
            _webSocketHandler.OnConnected += (sender, args) => { _logger.Information("Socket connected"); };
            _webSocketHandler.OnAuthenticated += (sender, args) =>
            {
                _logger.Information("Client authenticated with animus");
                _webSocketHandler.BeginReceiving();
            };

            await _webSocketHandler.Connect(stoppingToken);
      
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
                if ((_webSocketHandler.State == WebSocketState.Closed ||
                    _webSocketHandler.State == WebSocketState.Aborted ||
                    _webSocketHandler.State == WebSocketState.None )
                    && DateTime.Now > _slotCounter.NextSlotTimeStart)
                {
                    _slotCounter.CalculateNextSlotStart();
                    await _webSocketHandler.Connect(stoppingToken);
                }
            }
        }


        private void SetNextPingTime()
        {

            _nextPing = DateTime.Now.AddSeconds(30);
            _logger.Debug("Next pingtime {pingtime}", _nextPing);
        }


        private async Task CreateAndConfigureWebSocket()
        {

            //_ws = new WebSocket(uri.ToString(), "AHauth");

            //_ws.OnMessage += async (sender, e) =>
            //{
            //    if (e.Data != "authenticated")
            //    {
            //        _messageCounter.Tick();
            //        await _animusWebSocketEventHandler.Handle(e);
            //    }
            //    else
            //        _logger.Information("Laputa says: " + e.Data);
            //};



            //_ws.OnError += (sender, args) => { _logger.Error("Websocket reporting error"); };

            //_ws.OnClose += (sender, args) => { _logger.Information("Websocket is closing"); };
        }

        private RetryPolicy CreateRetryPolicy()
        {
            return Policy.Handle<WebSocketDisconnectException>()
                .WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(30),
            }, (ex, duration) =>
            {
                _logger.Error(ex, "Failed to connect to animus. Waiting {duration}", duration);
            });
        }

        //private void ReConnect(WebSocket ws)
        //{
        //    var auth = $"Authorization: Bearer {_animusConfiguration.Key}";
        //    _logger.Information("Trying to connect");

        //    var client = new ClientWebSocket();

        //    client.ConnectAsync(new Uri("ws://localhost:5000/ws"), CancellationToken.None).GetAwaiter().GetResult();


        //    //ws.Connect();
        //    //if (ws.ReadyState == WebSocketState.Closed || ws.ReadyState == WebSocketState.Closing)
        //    //    throw new WebSocketDisconnectException("Connection to animus still down after connect");

        //    //_ws.Send(auth);
        //}
    }
}
