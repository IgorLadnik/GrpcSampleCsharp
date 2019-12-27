using System;
using System.Threading;
using Grpc.Core;
using GrpcClientHelper;
using Communication;
using System.Text;

namespace GrpcClient
{
    public class Client : GrpcClientBase<RequestMessage, ResponseMessage>
    {
        private const int MsgIntervalInMs = 5000;
        private const int PayloadLength = 5;
        private const int NumOfResponses = 10;
        private const int NotToBeResponded = 7;
        private const int Important = 5;

        public string ClientId { get; }

        private readonly AutoResetEvent _ev = new AutoResetEvent(false);
        private readonly char[] _alphabet;
        private readonly Random _alphaIndex;
        private static int _currentMessageId = 0;

        public Client(Channel channel) 
            : base(channel)
        {
            ClientId = $"{Guid.NewGuid()}";
            _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            _alphaIndex = new Random();
        }

        public override AsyncDuplexStreamingCall<RequestMessage, ResponseMessage> CreateDuplexClient(Channel channel) =>
            new Messaging.MessagingClient(channel).CreateStreaming();

        public override RequestMessage CreateMessage(object ob)
        {
            var payload = $"{ob}";
            var messageId = _currentMessageId++;

            return new RequestMessage
            {
                ClientId = ClientId,
                MessageId = $"{messageId}",
                Type = payload.Contains('!') ? MessageType.Important : MessageType.Ordinary,
                Time = DateTime.UtcNow.Ticks,
                Response = payload.Contains('?') ? ResponseType.Required : ResponseType.NotRequired,
                Payload = payload
            };
        }

        public override string MessagePayload
        {
            //get => Console.ReadLine();
            get 
            {
                if (_ev.WaitOne(MsgIntervalInMs))
                    return string.Empty;
                else 
                {
                    var sb = new StringBuilder();
                    for (var i = 0; i < PayloadLength; i++)
                        sb.Append(_alphabet[_alphaIndex.Next(0, _alphabet.Length)]);
                    sb.Append(_currentMessageId % NumOfResponses == NotToBeResponded ? string.Empty : "?");
                    sb.Append(_currentMessageId % NumOfResponses == Important ? "!" : string.Empty);
                    return sb.ToString();
                }
            }
        }
    }
}
