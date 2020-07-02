using FluentAssertions;
using iRadio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace UnitTesting
{
    [TestClass]
    public class UnitTest1
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestCreateMultiPressCommands1()   // emtpy string
        {
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands("");
            Assert.IsTrue(mpc.Length == 0);
            // string sIN = TestContext.ResultsDirectory;    // "I:\\Thomas\\Sammlung\\Computer\\Programming\\Visual Studio 2012\\Projects\\iRadio\\TestResults\\Deploy_Thomas H. Schmidt 2020-06-11 18_03_47\\In"
            // string sOUT = TestContext.TestDeploymentDir;  // "I:\\Thomas\\Sammlung\\Computer\\Programming\\Visual Studio 2012\\Projects\\iRadio\\TestResults\\Deploy_Thomas H. Schmidt 2020-06-11 18_03_47\\Out"
        }

        private readonly MultiPressCommand[] Expected2 = new MultiPressCommand[] {
            new MultiPressCommand {Digit = 2, Times = 1} ,      // abc = "2" x1, x2, x3
            new MultiPressCommand {Digit = 2, Times = 2} ,
            new MultiPressCommand {Digit = 2, Times = 3}
        };
        [TestMethod]
        public void TestCreateMultiPressCommands2()    // string abc 
        {
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands("abc");
            Assert.IsTrue(mpc.Length == 3);
            mpc.Should().BeEquivalentTo(Expected2);
            // Assert.AreEqual(Expected2, mpc);  // tests for sameness, i.e. objects in array must be the same, not their contents

        }

        private readonly MultiPressCommand[] Expected3 = new MultiPressCommand[] {
            new MultiPressCommand {Digit = 2, Times = 1} ,      // aßc = "2" x1, ??, x3
            new MultiPressCommand {Digit = 2, Times = 3}
        };
        [TestMethod]
        public void TestCreateMultiPressCommands3()    // string with invalid char ß
        {
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands("aßc");
            Assert.IsTrue(mpc.Length == 2);
            mpc.Should().BeEquivalentTo(Expected3);
        }

        private readonly MultiPressCommand[] Expected4 = new MultiPressCommand[] {
            new MultiPressCommand {Digit = 2, Times = 1} ,
            new MultiPressCommand {Digit = 2, Times = 2} ,
            new MultiPressCommand {Digit = 2, Times = 3} ,
            new MultiPressCommand {Digit = 3, Times = 1} ,
            new MultiPressCommand {Digit = 3, Times = 2} ,
            new MultiPressCommand {Digit = 3, Times = 3} ,
            new MultiPressCommand {Digit = 4, Times = 1} ,
            new MultiPressCommand {Digit = 4, Times = 2} ,
            new MultiPressCommand {Digit = 4, Times = 3} ,
            new MultiPressCommand {Digit = 5, Times = 1}
        };
        [TestMethod]
        public void TestCreateMultiPressCommands4()     // string longer 10 chars
        {
            TestContext.WriteLine("CreateMultiPressCommands('abcdefghijklm')");
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands("abcdefghijklm");  // 13 chars
            Assert.IsTrue(mpc.Length == 10);
            mpc.Should().BeEquivalentTo(Expected4);
            for (int i = 0; i < mpc.Length; i++) TestContext.WriteLine("{0}", mpc[i]);
        }

        private readonly MultiPressCommand[] Expected5 = new MultiPressCommand[] {
            new MultiPressCommand {Digit = 2, Times = 1} ,
            new MultiPressCommand {Digit = 2, Times = 2} ,
            new MultiPressCommand {Digit = 2, Times = 3}
        };
        [TestMethod]
        public void TestCreateMultiPressCommands5()    // string ABC
        {
            TestContext.WriteLine("CreateMultiPressCommands('ABC')");
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands("ABC");
            Assert.IsTrue(mpc.Length == 3);
            mpc.Should().BeEquivalentTo(Expected5);
            for (int i = 0; i < mpc.Length; i++) TestContext.WriteLine("{0}", mpc[i]);
        }



        public class DebugNetworkStream : ITestableNetworkStream
        {
            private readonly StringBuilder WriteList = new StringBuilder();
            private string WriteLine = "";
            public Stream GetStream()
            {
                return new StreamReader("Telnet.xml").BaseStream;
            }
            public NetworkStream GetNetworkStream()
            {
                return null;
            }
            public bool CanWrite
            {
                get
                {
                    return true;
                }
            }
            public void Write([In, Out] byte[] buffer, int offset, int size)
            {
                string bufferstring = BitConverter.ToString(buffer);
                Debug.WriteLine("DebugNetworkStream.Write: {0}", bufferstring);
                WriteList.AppendLine(bufferstring);
                WriteLine = bufferstring;
            }
            public string LastWrite
            {
                get
                {
                    return WriteLine;
                }
            }
            public string AllWrites
            {
                get
                {
                    return WriteList.ToString();
                }
            }
            public int Read([In, Out] byte[] buffer, int offset, int size)
            {
                return 0;
            }
            public void Close()
            {
                ;
            }
        }

        [TestMethod]
        public void TestMacroStep()    // 
        {
            Noxon.netStream = new DebugNetworkStream();
            string[] m1s = new string[] { "L", "U", "R", "D" };
            Macro m1 = new iRadio.Macro("Test-m1", m1s);
            Macro m2 = new iRadio.Macro("Test-m2", new string[] { "N", "R", "R", "@hr3", "U", "D" });

            // run the two macros 'concurrently'
            bool ok = m1.Step();
            Assert.IsTrue(ok);
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['L'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ok = m1.Step();
            Assert.IsTrue(ok);
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['U'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ok = m2.Step();
            Assert.IsFalse(ok);  // must ignore Step() as macro m1 is still running, LastWrite still 'R', not 'N' (= first command of m2)
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['U'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ok = m2.Step();
            Assert.IsFalse(ok);  // must ignore Step() for m2
            int step = 2;
            do
            {
                ok = m1.Step();
                if (!ok) break;
                Assert.AreEqual(((DebugNetworkStream)Noxon.netStream).LastWrite, BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands[m1s[step++][0]].Key)));
            }
            while (ok);  // macro m1 finished

            ok = m2.Step();
            Assert.IsTrue(ok);  // must now process Step() for m2
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['N'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ok = m2.Step();
            Assert.IsTrue(ok);  
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['R'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ok = m2.Step();
            Assert.IsTrue(ok);  
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['R'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ok = m2.Step();
            Assert.IsTrue(ok);  // last key press for hr3 is '3' 
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['3'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ((DebugNetworkStream)Noxon.netStream).AllWrites.Should().EndWithEquivalent("00-00-00-34\r\n00-00-00-34\r\n00-00-00-37\r\n00-00-00-37\r\n00-00-00-37\r\n00-00-00-33\r\n00-00-00-33\r\n00-00-00-33\r\n00-00-00-33\r\n");
            ok = m2.Step();                                                         //  2x '4' = g(h)i                3x '7' = pq(r)                               4x '3' = def(3)
            Assert.IsTrue(ok);  
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['U'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);
            ok = m2.Step();
            Assert.IsTrue(ok);
            Assert.AreEqual(BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands['D'].Key)), ((DebugNetworkStream)Noxon.netStream).LastWrite);

            var mock = new Mock<ITestableNetworkStream>();
            mock.Setup(stream => stream.CanWrite).Returns(true);
            Noxon.netStream = mock.Object;            // https://github.com/moq/moq4
            bool write = Noxon.netStream.CanWrite;
            Assert.IsTrue(write);
        }
    }
}
