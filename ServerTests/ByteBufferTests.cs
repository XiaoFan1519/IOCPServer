using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tests
{
    [TestClass()]
    public class ByteBufferTests
    {

        [TestMethod()]
        public void ReadableBytesTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(1);
            byteBuffer.WriteInt(1);
            Assert.AreEqual(sizeof(int), byteBuffer.ReadableBytes());
        }

        [TestMethod()]
        public void WritableBytesTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.WriteInt(1);
            byte[] arr = byteBuffer.GetBytes(0, sizeof(int));
            Assert.AreEqual(4, arr.Length);
            Assert.AreEqual(1, arr[3]);
        }

        [TestMethod()]
        public void ClearTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.WriteInt(1);
            Assert.AreEqual(4, byteBuffer.ReadableBytes());
            byteBuffer.Clear();
            Assert.AreEqual(0, byteBuffer.ReadableBytes());
        }

        [TestMethod()]
        public void EnsureWritableTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(3);
            Assert.AreEqual(3, byteBuffer.Capacity);
            byteBuffer.WriteInt(1);
            Assert.IsTrue(byteBuffer.Capacity > 3);
        }

        [TestMethod()]
        public void WriteBytesTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(4);
            Assert.AreEqual(4, byteBuffer.WritableBytes());
            byteBuffer.WriteInt(1);
            Assert.AreEqual(0, byteBuffer.WritableBytes());
            Assert.AreEqual(4, byteBuffer.WriterIndex);
        }

        [TestMethod()]
        public void WriteShortTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(4);
            byteBuffer.WriteShort(1);
            byte[] buff = byteBuffer.GetBytes(0, 2);
            Assert.AreEqual(1, buff[1]);
            Assert.AreEqual(2, byteBuffer.WriterIndex);
        }

        [TestMethod()]
        public void WriteIntTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(4);
            byteBuffer.WriteInt(15);
            byte[] buff = byteBuffer.GetBytes(0, 4);
            Assert.AreEqual(15, buff[3]);
            Assert.AreEqual(4, byteBuffer.WriterIndex);
        }

        [TestMethod()]
        public void GetBytesTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(4);
            byteBuffer.WriteInt(2147483647);
            byte[] buff = byteBuffer.GetBytes(0, 4);
            Assert.AreEqual(127, buff[0]);
            Assert.AreEqual(255, buff[1]);
            Assert.AreEqual(255, buff[2]);
            Assert.AreEqual(255, buff[3]);
            Assert.AreEqual(4, byteBuffer.WriterIndex);

            // 越界
            Assert.ThrowsException<ArgumentException>(() =>
            {
                byteBuffer.GetBytes(5, 4);
            });
        }

        [TestMethod]
        public void ReadBytesTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(4);
            Assert.ThrowsException<IndexOutOfRangeException>(() =>
            {
                byteBuffer.ReadBytes(5);
            });
            byteBuffer.WriteInt(2147483647);
            byte[] buff = byteBuffer.ReadBytes(4);
            Assert.AreEqual(4, byteBuffer.ReaderIndex);
            Assert.AreEqual(127, buff[0]);
            Assert.AreEqual(255, buff[1]);
            Assert.AreEqual(255, buff[2]);
            Assert.AreEqual(255, buff[3]);
            Assert.AreEqual(4, buff.Length);
        }

        [TestMethod]
        public void ReadShortTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(2);
            byteBuffer.WriteShort(127);
            Assert.AreEqual(127, byteBuffer.ReadShort());
        }

        [TestMethod]
        public void ReadIntTest()
        {
            ByteBuffer byteBuffer = new ByteBuffer(4);
            byteBuffer.WriteInt(2147483647);
            Assert.AreEqual(2147483647, byteBuffer.ReadInt());
        }
    }
}