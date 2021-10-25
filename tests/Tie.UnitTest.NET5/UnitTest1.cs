using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tie;

namespace Tie.UnitTest.NET5
{
    [TestClass]
    public class UnitTest1
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

    }
}
