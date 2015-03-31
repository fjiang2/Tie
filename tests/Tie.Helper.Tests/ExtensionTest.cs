using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Tie.Helper.Tests
{
    public class ExtensionTest
    {
        [Fact]
        public void TestToVAL()
        {
            //Arrange
            var address = new { Name="1000 SH 6", City = "Sugar Land", State = "TX", Zip = "77578"};

            // Arrange
            string code = "{ Name : '1000 SH 6', City : 'Sugar Land', State : 'TX', Zip : '77578'}";

            // Act
            var val1 = address.ToVAL();
            var val2 = Script.Evaluate(code);

            // Assert
            Assert.Equal(val1.ToString(), val2.ToString());
        }

    }
}
