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
    public class BasicFacts
    {

        [Fact]
        public void TestStringEscape()
        {
            // Arrange
            var builder = new StringBuilder();
            builder.AppendLine("line1");
            builder.AppendLine("line2");
            
            VAL str = new VAL(builder.ToString());
            string json = str.ToJson();
            
            // Act
            VAL val = Script.Evaluate(json);

            // Assert
            Assert.Equal(builder.ToString(), val.Str);
        }

      

    }
}
