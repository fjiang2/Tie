using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tie.Tests
{
    [TestClass]
    public class BasicFacts
    {

        [TestMethod]
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
            Assert.AreEqual(builder.ToString(), val.Str);
        }

        [TestMethod]
        public void TestTokenize()
        {
            //Arrange
            string path = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools /all /has:true";

            //Act
            IEnumerable<token> L = Script.Tokenize(path);

            string text = string.Join("|", L.Select(x => x.tok));
            //Assert
            Assert.AreEqual(text, @"C|:|\|Program|Files|(|x86|)|\|Microsoft|Visual|Studio|12.0|\|Common7|\|Tools|/|all|/|has|:|true");
        }

        [TestMethod]
        public void TestTokenizeString()
        {
            //Arrange
            string path = "\"C:\\\\Program Files (x86)\\\\Microsoft Visual Studio 12.0\\\\Common7\\\\Tools\" /all";

            //Act
            IEnumerable<token> L = Script.Tokenize(path);

            string text = string.Join("|", L.Select(x => x.tok));
            //Assert
            Assert.AreEqual(text, @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools|/|all");
        }

        [TestMethod]
        public void TestTokenizeExtensionString()
        {
            //Arrange
            //              string message = $"Application encountering error: {e.Exception.Message}.";
            string code = "string message = $\"Application encountering error: {e.Exception.Message}.\"";

            //Act
            IEnumerable<token> L = Script.Tokenize(code);

            string text = string.Join("|", L.Select(x => x.tok));
            //Assert
            Assert.AreEqual(text, @"string|message|=|$|Application encountering error: {e.Exception.Message}.");
        }


        [TestMethod]
        public void TestTokenizeSignleQuoteString()
        {
            //Arrange
            //   "Please press \'Download Clock\'"
            string code = "\"Please press \'Download\'\"";

            //Act
            IEnumerable<token> L = Script.Tokenize(code);

            string text = string.Join("|", L.Select(x => x.tok));
            //Assert
            Assert.AreEqual(text, @"Please press 'Download'");


            code = "\"Please press \'Download\"";
            //Act
            L = Script.Tokenize(code);

            text = string.Join("|", L.Select(x => x.tok));
            //Assert
            Assert.AreEqual(text, @"Please press 'Download");

        }


        [TestMethod]
        public void TestTokenizeFile()
        {
            string path = @"..\..\..\..\Tie\Compiler\Exception\Error.cs";
            string code = File.ReadAllText(path);

            //Act
            IEnumerable<token> L = Script.Tokenize(code);

            var L2 = L.Where(x => x.ty == tokty.stringcon).ToArray();
            string text = string.Join(Environment.NewLine, L2.Select(x => x.tok));

            //Assert
            Debug.Assert(text.EndsWith("Symbol Table overflow."));
        }


    }
}
