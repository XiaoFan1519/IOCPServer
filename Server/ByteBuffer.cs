﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IOCP
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
        /// 获取当前可以写入的字节数
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
        /// 将指定数量的字节数组写入ByteBuffer中
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public ByteBuffer WriteByte(byte b)
        {
            EnsureWritable(1);
            buffer[writerIndex] = b;
            writerIndex += 1;
            return this;
        }

        /// <summary>
        /// 将指定数量的字节数组写入ByteBuffer中
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public ByteBuffer WriteBytes(byte[] bytes, int offset, int count)
        {
            EnsureWritable(count);
            Buffer.BlockCopy(bytes, offset, buffer, writerIndex, count);
            writerIndex += count;
            return this;
        }

        /// <summary>
        /// 将字节数组写入ByteBuffer中
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public ByteBuffer WriteBytes(byte[] bytes)
        {
            return WriteBytes(bytes, 0, bytes.Length);
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
        /// 在不影响readerIndex的情况下获取指定位置的字节
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte Peek(int index)
        {
            // 越界检测
            if (IsOutOfBounds(index, 1, capacity))
            {
                throw new ArgumentException(string.Format("index:{0} 无效", index));
            }

            return buffer[index];
        }

        /// <summary>
        /// 在不影响readerIndex的情况下获取字节数组
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">获取数量</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">指定的参数越界</exception>
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

        /// <summary>
        /// 从缓存中读取一个Short
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBytes(int count)
        {
            if (ReadableBytes() < count)
            {
                throw new IndexOutOfRangeException(string.Format("Read Count:{0}, But ReadableBytes:{1}", count, ReadableBytes()));
            }

            byte[] buff = GetBytes(readerIndex, count);
            readerIndex += count;
            return buff;
        }

        /// <summary>
        /// 从缓存中读取一个Short
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            byte[] buff = GetBytes(readerIndex, 2);
            return (short)(buff[0] << 8 | buff[1] & 0xFF);
        }

        /// <summary>
        /// 从缓存中读取一个Short
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            byte[] buff = GetBytes(readerIndex, 4);
            return buff[0] << 24 | 
                buff[1] << 16 |
                buff[2] <<   8 |
                buff[3];
        }
    }
}
