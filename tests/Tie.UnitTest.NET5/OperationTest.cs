using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tie;

namespace Tie.UnitTest.NET5
{
    [TestClass]
    public class OperationTest
    {
        [TestMethod]
        public void Test_int_operation()
        {
            string code = @"a = 11;
b = 2;
c = 15;
d = (decimal)100.1;

z1 = a > b;
z2 = a < c;
z3 = a < d;
";

            Memory ds = new Memory();
            Script.Execute(code, ds);
            Assert.IsTrue((bool)ds["z1"]);
            Assert.IsTrue((bool)ds["z2"]);
            Assert.IsTrue((bool)ds["z3"]);

        }

        [TestMethod]
        public void Test_double_operation()
        {
            string code = @"a = 11.12;
b = 11.55;
c = 3;
d = (decimal)100.1;

z1 = a < b;
z2 = a > c;
z3 = a < d;
";

            Memory ds = new Memory();
            Script.Execute(code, ds);
            Assert.IsTrue((bool)ds["z1"]);
            Assert.IsTrue((bool)ds["z2"]);
            Assert.IsTrue((bool)ds["z3"]);

        }

        [TestMethod]
        public void Test_decimal_operation()
        {
            string code = @"a = (decimal)11.12;
b = 11.55;
c = 3;
d = (decimal)100.1;

z1 = a < b;
z2 = a > c;
z3 = a < d;
";

            Memory ds = new Memory();
            Script.Execute(code, ds);
            Assert.IsTrue((bool)ds["z1"]);
            Assert.IsTrue((bool)ds["z2"]);
            Assert.IsTrue((bool)ds["z3"]);

        }

        [TestMethod]
        public void Test_bitwise_operation()
        {
            string code = @"a = 12;
b = 159;
z1 = a & b;
z2 = a | b;
z3 = ~a;
";
            int a = 12;
            int b = 159;
            int z1 = a & b;
            int z2 = a | b;
            int z3 = ~a;

            Memory ds = new Memory();
            Script.Execute(code, ds);
            Assert.AreEqual((int)ds["z1"], z1);
            Assert.AreEqual((int)ds["z2"], z2);
            Assert.AreEqual((int)ds["z3"], z3);

        }

        [TestMethod]
        public void Test_Tokenize()
        {
            string text = "@a=12;";
            var tokens = Script.Tokenize(text).ToArray();
            Assert.AreEqual("@a", tokens[0].tok);
            Assert.AreEqual(";", tokens[3].tok);

        }

    }
}
