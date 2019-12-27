using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace GrpcClientHelper
{
    public abstract class GrpcClientBase<TRequest, TResponse>
    {
        public abstract AsyncDuplexStreamingCall<TRequest, TResponse> CreateDuplexClient(Channel channel);

        public abstract TRequest CreateMessage(object ob);

        public abstract string MessagePayload { get; }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _task;
        private Channel _channel;
        private Action _onShuttingDown;

        protected GrpcClientBase(Channel channel) => _channel = channel;
        
        public void DoAsync(Action<TRequest> onSend, Action<TResponse> onReceive,
                            Action onConnection = null, Action onShuttingDown = null)
        {
            _onShuttingDown = onShuttingDown;

            _task = Task.Run(async () =>
            {
                using var duplex = CreateDuplexClient(_channel);

                onConnection?.Invoke();

                var responseTask = Task.Run(async () =>
                {
                    while (await duplex.ResponseStream.MoveNext(_cts.Token))
                        onReceive(duplex.ResponseStream.Current);
                });

                string payload;
                while (!_cts.Token.IsCancellationRequested && 
                       !string.IsNullOrEmpty(payload = MessagePayload))
                {
                    var request = CreateMessage(payload);
                    onSend(request);
                    await duplex.RequestStream.WriteAsync(request);
                }

                TheEnd();
            });
        }

        private async void TheEnd() 
        {
            _onShuttingDown?.Invoke();
            await _channel.ShutdownAsync();
        }

        public void Stop() => StopAsync().Wait(5000);

        private async Task StopAsync() 
        {
            _cts.Cancel();
            try
            {
                await _task;
            }
            catch (OperationCanceledException) 
            {
                TheEnd();
            }
        } 
    }
}


