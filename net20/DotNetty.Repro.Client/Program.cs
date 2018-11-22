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


            EndPoint host = null;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNETTY_REPRO_HOST")))
            {
                var hostname = Environment.GetEnvironmentVariable("DOTNETTY_REPRO_HOST");
                Console.WriteLine("Connected to host {0}", hostname);
                host = new DnsEndPoint(hostname, 1099);

                // debug - enumerate all hostnames resolved
                Dns.GetHostAddressesAsync(hostname)
                    .ContinueWith(tr =>
                    {
                        foreach(var r in tr.Result)
                            Console.WriteLine("Found entry for {0} {1}", hostname, r);
                    });
            }
            else
            {
                host = new DnsEndPoint("localhost", 1099);
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

            IChannel bootstrapChannel = bootstrap.ConnectAsync(host).Result;

            Console.CancelKeyPress += (sender, eventArgs) => bootstrapChannel.CloseAsync();

            bootstrapChannel.CloseCompletion.Wait();
            group.ShutdownGracefullyAsync().Wait(1000);
        }
    }
}
