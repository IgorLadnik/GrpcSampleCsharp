using System;
using System.IO;
using Grpc.Core;

namespace GrpcClient
{
    class Program
    {
        const int PORT = 19019;

        static readonly string nl = Environment.NewLine;

        static void Main(string[] args)
        {
            Console.WriteLine("GrpcClient started.");

            var channelCredentials = new SslCredentials(File.ReadAllText(@"./certs/ca.crt"),
                new KeyCertificatePair(File.ReadAllText("./certs/client.crt"), File.ReadAllText(@"./certs/client.key")));        

            var client = new Client(new Channel($"localhost:{PORT}", channelCredentials));
            client.DoAsync(
                onSend: request => Console.WriteLine($"{nl}Request:  {request}"),
                onReceive: response => Console.WriteLine($"Response: {response}"),
                onConnection: () =>
                {
                    var orgTextColor = Console.ForegroundColor;
                    Console.Write($"Connected to server.{nl}ClientId = ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{client.ClientId}");
                    Console.ForegroundColor = orgTextColor;
                    Console.WriteLine($".{nl}Press any key to quit...{nl}");
                },
                onShuttingDown: () => Console.WriteLine("Shutting down...")
            );

            Console.ReadKey();
            client.Stop();
        }
    }
}
