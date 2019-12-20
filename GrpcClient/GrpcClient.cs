using System;
using Grpc.Core;
using GrpcClientHelper;
using Communication;

namespace GrpcClient
{
    public class Client : GrpcClientBase<RequestMessage, ResponseMessage>
    {
        public string ClientId { get; }

        private int messageId = 0;

        public Client() => ClientId = $"{Guid.NewGuid()}";
        
        public override AsyncDuplexStreamingCall<RequestMessage, ResponseMessage> CreateDuplexClient(Channel channel) =>
            new Messaging.MessagingClient(channel).CreateStreaming();

        public override RequestMessage CreateMessage(object ob)
        {
            var payload = $"{ob}";

            return new RequestMessage
            {
                ClientId = ClientId,
                MessageId = $"{messageId++}", // $"{Guid.NewGuid()}",
                Type = MessageType.Ordinary,
                Time = DateTime.UtcNow.Ticks,
                Response = payload.Contains('?') ? ResponseType.Required : ResponseType.NotRequired,
                Payload = payload
            };
        }

        public override string MessagePayload => Console.ReadLine();
    }
}
