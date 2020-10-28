﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tie;

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
            string path = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools /all";

            //Act
            IEnumerable<token> L = Script.Tokenize(path);

            //Assert
            Assert.AreEqual(string.Join("|", L.Select(x=>x.tok)), @"C|:|\|Program|Files|(|x86|)|\|Microsoft|Visual|Studio|12.0|\|Common7|\|Tools|/|all");
        }

        [TestMethod]
        public void TestTokenizeString()
        {
            //Arrange
            string path = "\"C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\Tools\" /all";

            //Act
            IEnumerable<token> L = Script.Tokenize(path);

            //Assert
            Assert.AreEqual(string.Join("|", L.Select(x => x.tok)), @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools|/|all");
        }

    }
}
