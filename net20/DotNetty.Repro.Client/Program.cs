using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace DotNetty.Repro.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting up discard client");

            var group = new MultithreadEventLoopGroup();


            IPAddress host = null;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNETTY_REPRO_HOST")))
            {
                host = Dns.GetHostAddressesAsync(Environment.GetEnvironmentVariable("DOTNETTY_REPRO_HOST")).Result.FirstOrDefault();
            }
            else
            {
                host = IPAddress.Parse("127.0.0.1");
            }

            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    pipeline.AddLast(new LoggingHandler());
                    pipeline.AddLast(new DiscardClientHandler());
                }));

            IChannel bootstrapChannel = bootstrap.ConnectAsync(host, 1099).Result;

            Console.CancelKeyPress += (sender, eventArgs) => bootstrapChannel.CloseAsync();

            bootstrapChannel.CloseCompletion.Wait();
            group.ShutdownGracefullyAsync().Wait(1000);
        }
    }
}
