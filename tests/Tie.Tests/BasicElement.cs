using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Tie;

namespace Tie.Tests
{
    public class BasicElement
    {
        [Fact]
        public void EscapedString()
        { 
            //Arrange
            string code = @"@'\\192.168.0.1\shared'";

            //Act
            var val = Script.Evaluate(code);

            // Assert
            Assert.Equal(val.ToSimpleString(), "\\\\192.168.0.1\\shared");
        }

        [Fact]
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
            Assert.Equal((string)DS["I0"].Valor, "{1,2,3}");
            Assert.Equal((string)DS["I1"].Valor, "{1,2,3}.typeof(System.Int32[])");
            Assert.Equal((string)DS["I2"].Valor, "{}.typeof(System.String[])");

            VAL x = Script.Evaluate("{1,2,3}.typeof(int[])");
            Assert.Equal((string)DS["I1"].Valor, x.Valor);
            Logger.Close();
        }


        [Fact]
        public void AnonymousClass()
        {
            // Arrange
            string code1 = "new { Name='1000 SH 6', City = 'Sugar Land', State = 'TX', Zip = '77578'}";
            string code2 = "{ Name : '1000 SH 6', City : 'Sugar Land', State : 'TX', Zip : '77578'}";

            // Act
            var val1 = Script.Evaluate(code1);
            var val2 = Script.Evaluate(code2);

            // Assert
            Assert.Equal(val1.ToString(), val2.ToString());
        }

    }
}
