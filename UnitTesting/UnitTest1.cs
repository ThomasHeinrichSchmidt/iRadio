using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using iRadio;

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
            string sIN = TestContext.ResultsDirectory;    // "I:\\Thomas\\Sammlung\\Computer\\Programming\\Visual Studio 2012\\Projects\\iRadio\\TestResults\\Deploy_Thomas H. Schmidt 2020-06-11 18_03_47\\In"
            string sOUT = TestContext.TestDeploymentDir;  // "I:\\Thomas\\Sammlung\\Computer\\Programming\\Visual Studio 2012\\Projects\\iRadio\\TestResults\\Deploy_Thomas H. Schmidt 2020-06-11 18_03_47\\Out"
        }

        private MultiPressCommand[] Expected2 = new MultiPressCommand[] {
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

        private MultiPressCommand[] Expected3 = new MultiPressCommand[] {
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

        private MultiPressCommand[] Expected4 = new MultiPressCommand[] {
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

        private MultiPressCommand[] Expected5 = new MultiPressCommand[] {
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
    }
}
