using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ByteBuffer
    {
        private byte[] buffer;

        private int readerIndex;
        private int writerIndex;

        public int ReaderIndex => readerIndex;
        public int WriterIndex => writerIndex;
    }
}
