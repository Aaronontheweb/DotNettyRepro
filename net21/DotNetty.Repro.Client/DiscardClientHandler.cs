using System;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace DotNetty.Repro.Client
{
    public class DiscardClientHandler : SimpleChannelInboundHandler<object>
    {
        IChannelHandlerContext _ctx;
        byte[] _array;

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            this._array = new byte[12];
            this._ctx = ctx;

            Console.WriteLine("Connected to server");
        }

        protected override void ChannelRead0(IChannelHandlerContext context, object message)
        {
            // Server is supposed to send nothing, but if it sends something, discard it.
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.WriteLine("{0}", e.ToString());
            this._ctx.CloseAsync();
        }
    }
}