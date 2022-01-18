using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tie;

namespace Tie.UnitTest.NET5
{
    [TestClass]
    public class BasicElement
    {
        [TestMethod]
        public void EscapedString()
        { 
            //Arrange
            string code = @"@'\\192.168.0.1\shared'";

            //Act
            var val = Script.Evaluate(code);

            // Assert
            Assert.AreEqual(val.ToSimpleString(), "\\\\192.168.0.1\\shared");
        }

        [TestMethod]
        public void ArrayTest()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();

            string code = @"
               I0 = {1,2,3}; 
               I1 = new int[]{1,2,3};
               I2 = new string[]; 
               int64 = long[];
               I3 = new int64 {10,20,30};
               I4 = new object[] {'A', 1, 3.0};
               A = new int[][] { {1,2}, {3,4}, {5,6}};
            ";

            DS.RemoveAll();
            Script.Execute(code, DS);
            Assert.AreEqual((string)DS["I0"].Valor, "{1,2,3}");
            Assert.AreEqual((string)DS["I1"].Valor, "{1,2,3}.typeof(System.Int32[])");
            Assert.AreEqual((string)DS["I2"].Valor, "{}.typeof(System.String[])");

            VAL x = Script.Evaluate("{1,2,3}.typeof(int[])");
            Assert.AreEqual((string)DS["I1"].Valor, x.Valor);
            Logger.Close();
        }


        [TestMethod]
        public void AnonymousClass()
        {
            // Arrange
            string code1 = "new { Name='1000 SH 6', City = 'Sugar Land', State = 'TX', Zip = '77578'}";
            string code2 = "{ Name : '1000 SH 6', City : 'Sugar Land', State : 'TX', Zip : '77578'}";

            // Act
            var val1 = Script.Evaluate(code1);
            var val2 = Script.Evaluate(code2);

            // Assert
            Assert.AreEqual(val1.ToString(), val2.ToString());
        }

        [TestMethod]
        public void AnonymousNetClass()
        {
            // Arrange
            var code1 = new { Name="1000 SH 6", City = "Sugar Land", State = "TX", Zip = "77578"};
            string code2 = "{ Name : '1000 SH 6', City : 'Sugar Land', State : 'TX', Zip : '77578'}";

            // Act
            var val1 = Valizer.Valize(code1);
            var val2 = Script.Evaluate(code2);

            // Assert
            Assert.AreEqual(val1.ToJson(null), val2.ToJson(null));
        }

        [TestMethod]
        public void TestCastValue()
        {
            // Arrange
            var code1 = "a=2.3; b=(int)a;";

            // Act
            var DS = new Memory();
            var val2 = Script.Execute(code1, DS);

            // Assert
            Assert.AreEqual((int)DS["a"], 2);
            Assert.AreEqual(DS["b"].Value, 2);
            Assert.AreEqual((double)DS["b"], 2.0);
        }

        [TestMethod]
        public void TestStringGetCharArray()
        {
            // Arrange
            var code1 = "S='abc'; a=S[0]; b=S[1]; c=S[3]; d=S[-1]; ";

            // Act
            var DS = new Memory();
            var val2 = Script.Execute(code1, DS);

            // Assert
            Assert.AreEqual(DS["a"].Value, "a");
            Assert.AreEqual(DS["b"].Value, "b");
            Assert.AreEqual(DS["c"], VAL.VOID);
            Assert.AreEqual(DS["d"], VAL.VOID);
        }


        [TestMethod]
        public void TestAssociativeVAL()
        {
            // Arrange
            var code1 = "{A:'abc', B:1, C:true}";

            // Act
            var val = Script.Evaluate(code1);

            // Assert
            Dictionary<string, object> dict = new Dictionary<string,object>();
            foreach (Member m in val.Members)
            {
                dict.Add(m.Name, m.Value.Value);
            }

            Assert.AreEqual(dict["A"], "abc");
            Assert.AreEqual(dict["B"], 1);
            Assert.AreEqual(dict["C"], true);
        }
      
    }
}
