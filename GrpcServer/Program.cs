using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
//using System.Security.Cryptography.X509Certificates;
//using System.Net.Http;

namespace GrpcServer
{
    public class Program
    {
        const int PORT = 19019;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;
                        options.Listen(IPAddress.Any, PORT,
                        listenOptions =>
                        {
                            listenOptions.UseHttps("./certs/server.pfx", "1234");
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        });
                    });
                });
    }
}
