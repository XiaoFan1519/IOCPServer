﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    interface IHandle
    {
        void ChannelRead(Channel channel, ByteBuffer msg);

        void ExceptionCaught(Channel channel, Exception cause);
    }
}
