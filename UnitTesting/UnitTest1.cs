using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace UnitTesting
{
    [TestClass]
    public class UnitTest1
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestCreateMultiPressCommands1()
        {
            iRadio.Program.MultiPressCommand[] mpc = iRadio.Program.CreateMultiPressCommands("");
            Assert.IsTrue(mpc.Length == 0);
        }

        private iRadio.Program.MultiPressCommand[] Expected2 = new iRadio.Program.MultiPressCommand[] {
            new iRadio.Program.MultiPressCommand {Digit = 2, Times = 1} ,      // abc = "2" x1, x2, x3
            new iRadio.Program.MultiPressCommand {Digit = 2, Times = 2} ,
            new iRadio.Program.MultiPressCommand {Digit = 2, Times = 3}
        };
        [TestMethod]
        public void TestCreateMultiPressCommands2()
        {
            iRadio.Program.MultiPressCommand[] mpc = iRadio.Program.CreateMultiPressCommands("abc");
            Assert.IsTrue(mpc.Length == 3);
            mpc.Should().BeEquivalentTo(Expected2);
            // Assert.AreEqual(Expected2, mpc);  // tests for sameness, i.e. objects in array must be the same, not their contents

        }

        private iRadio.Program.MultiPressCommand[] Expected3 = new iRadio.Program.MultiPressCommand[] {
            new iRadio.Program.MultiPressCommand {Digit = 2, Times = 1} ,      // aßc = "2" x1, ??, x3
            new iRadio.Program.MultiPressCommand {Digit = 2, Times = 3}
        };
        [TestMethod]
        public void TestCreateMultiPressCommands3()
        {
            iRadio.Program.MultiPressCommand[] mpc = iRadio.Program.CreateMultiPressCommands("aßc");
            Assert.IsTrue(mpc.Length == 2);
            mpc.Should().BeEquivalentTo(Expected3);
            String sIN = TestContext.ResultsDirectory;    // "I:\\Thomas\\Sammlung\\Computer\\Programming\\Visual Studio 2012\\Projects\\iRadio\\TestResults\\Deploy_Thomas H. Schmidt 2020-06-11 18_03_47\\In"
            String sOUT = TestContext.TestDeploymentDir;  // "I:\\Thomas\\Sammlung\\Computer\\Programming\\Visual Studio 2012\\Projects\\iRadio\\TestResults\\Deploy_Thomas H. Schmidt 2020-06-11 18_03_47\\Out"
        }
    }
}
