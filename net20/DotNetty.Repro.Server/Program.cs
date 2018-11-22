using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace DotNetty.Repro.Server
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        private int _numChannels = 0;

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _numChannels++;
            Console.WriteLine("Have {0} active connections", _numChannels);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _numChannels--;
            Console.WriteLine("Have {0} active connections", _numChannels);
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            // discard
        }

        public override bool IsSharable => true;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on port 1099");

            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();

            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .Handler(new LoggingHandler("SRV"))
                .ChildHandler(new ServerHandler());

            IChannel boundChannel = bootstrap.BindAsync(IPAddress.Any, 1099).Result;
            Console.WriteLine("Bound to {0}", boundChannel.LocalAddress);

            Console.CancelKeyPress += (sender, eventArgs) => boundChannel.CloseAsync();

            boundChannel.CloseCompletion.Wait();
            Console.WriteLine("Channel terminated");
            Task.WhenAll(
                 bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                 workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))).Wait(TimeSpan.FromMilliseconds(3000));
        }
    }
}
