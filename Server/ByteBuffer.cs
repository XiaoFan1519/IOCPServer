using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ByteBuffer
    {
        /// <summary>
        /// 容量
        /// </summary>
        private int capacity;

        private byte[] buffer;
        
        private int readerIndex;
        
        private int writerIndex;

        /// <summary>
        /// 当前容量
        /// </summary>
        public int Capacity => capacity;

        /// <summary>
        /// 读索引
        /// </summary>
        public int ReaderIndex => readerIndex;

        /// <summary>
        /// 写索引
        /// </summary>
        public int WriterIndex => writerIndex;
        
        public ByteBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new byte[capacity];
        }

        public ByteBuffer() : this(10240)
        {
            
        }

        /// <summary>
        /// Returns the number of readable bytes which is equal to
        /// writerIndex - readerIndex
        /// </summary>
        /// <returns></returns>
        public int ReadableBytes()
        {
            return writerIndex - readerIndex;
        }

        /// <summary>
        /// 获取当前可以写如的字节数
        /// </summary>
        /// <returns></returns>
        public int WritableBytes()
        {
            return capacity - writerIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ByteBuffer Clear()
        {
            readerIndex = writerIndex = 0;
            return this;
        }

        /// <summary>
        /// 确保可写
        /// </summary>
        /// <param name="length">长度</param>
        public void EnsureWritable(int length)
        {
            if (length <= WritableBytes())
            {
                return;
            }

            // 扩容
            int newLength = capacity + length * 2;
            byte[] buffer = new byte[newLength];
            Buffer.BlockCopy(this.buffer, 0, buffer, 0, writerIndex);
            capacity = newLength;
            this.buffer = buffer;
        }

        /// <summary>
        /// 将字节数组写入ByteBuffer中
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public ByteBuffer WriteBytes(byte[] bytes)
        {
            EnsureWritable(bytes.Length);
            Buffer.BlockCopy(bytes, 0, buffer, writerIndex, bytes.Length);
            writerIndex += bytes.Length;
            return this;
        }

        /// <summary>
        /// 将short按照网络字节序写入到ByteBuffer中
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ByteBuffer WriteShort(short value)
        {
            value = IPAddress.HostToNetworkOrder(value);
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
            return this;
        }

        /// <summary>
        /// 将int按照网络字节序写入到ByteBuffer中
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ByteBuffer WriteInt(int value)
        {
            value = IPAddress.HostToNetworkOrder(value);
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
            return this;
        }

        /// <summary>
        /// 越界检测
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        private static bool IsOutOfBounds(int index, int length, int capacity)
        {
            return (index | length | (index + length) | (capacity - (index + length))) < 0;
        }

        /// <summary>
        /// 在不影响readerIndex的情况下获取字节
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">获取数量</param>
        /// <returns></returns>
        public byte[] GetBytes(int index, int count)
        {
            // 越界检测
            if (IsOutOfBounds(index, count, capacity))
            {
                throw new ArgumentException(string.Format("index:{0} 或 count:{1} 无效", index, count));
            }

            byte[] buff = new byte[count];
            Buffer.BlockCopy(buffer, index, buff, 0, count);
            return buff;
        }
    }
}
