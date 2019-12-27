using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GrpcServerHelper;
using Communication;

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;


namespace GrpcServer
{
    public class MessageProcessor : MessageProcessorBase<RequestMessage, ResponseMessage>
    {
        private IList<ResponseMessage> _lstRespMsg;

        public MessageProcessor(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            var arrJ = JsonConvert.DeserializeObject(File.ReadAllText("data.json")) as JArray;

            _lstRespMsg = new List<ResponseMessage>();
            foreach (var jOb in arrJ)
            {
                var jResponse = jOb["response"];             
                _lstRespMsg.Add(new ResponseMessage
                {
                    MessageId = jResponse["messageId"].Value<string>(),
                    Type = (MessageType)jResponse["type"].Value<string>().ToProtoEnum<MessageType>(),
                    Payload = jResponse["payload"].Value<string>()
                });
            }
        }

        public override string GetClientId(RequestMessage request) => request.ClientId;

        public override ResponseMessage Process(RequestMessage request)
        {
            if (string.IsNullOrEmpty(request.Payload))
                return null;

            Logger.LogInformation($"Request:  {request}");

            //
            // Request message processing should be placed here
            //

            if (request.Response != ResponseType.Required)
                return null;

            var responseMessage = int.TryParse(request.MessageId, out var intMessageId)
                ? _lstRespMsg[intMessageId % _lstRespMsg.Count]
                : null;

            if (responseMessage == null)
                return new ResponseMessage
                {
                    ClientId = request.ClientId,
                    MessageId = request.MessageId,
                    Type = request.Type,
                    Time = DateTime.UtcNow.Ticks,
                    Payload = "Error in MessageId",
                    Status = MessageStatus.Error
                };

            responseMessage.ClientId = request.ClientId;
            responseMessage.MessageId = request.MessageId;
            responseMessage.Time = DateTime.UtcNow.Ticks;
            responseMessage.Status = MessageStatus.Processed;

            return responseMessage;
        }
    }

    public static class ProtoStringEx
    {
        public static int ToProtoEnum<T>(this string protoString)
        {
            var typeEnum = typeof(T);
            if (!typeEnum.IsEnum || string.IsNullOrEmpty(protoString))
                return 0;

            var sProto = protoString.ToLower().Replace('_', '.');
            var typeName = typeEnum.Name.ToLower();
            var ss = typeEnum.GetEnumNames();
            for (var i = 0; i < ss.Length; i++)
                if ($"{typeName}.{ss[i].ToLower()}" == sProto)
                    return i;

            return 0;
        }
    }
}
