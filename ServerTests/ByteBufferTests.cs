using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            Assert.Fail();
        }

        [TestMethod()]
        public void EnsureWritableTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void WriteBytesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void WriteShortTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void WriteIntTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsOutOfBoundsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetBytesTest()
        {
            Assert.Fail();
        }
    }
}