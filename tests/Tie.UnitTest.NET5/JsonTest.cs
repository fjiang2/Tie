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
    public class JsonTest
    {
        [TestMethod]
        public void DictinaryToJson()
        {
            //Arrange
            var dict = new Dictionary<string, object>();
            var list = new List<string>();

            dict.Add("Name", "1000 SH 6");
            dict.Add("City", "Sugar Land");
            dict.Add("State", "TX");
            dict.Add("Zip", "77578");
            dict.Add("Friends", list);
            list.Add("Sam");
            list.Add("Ana");

            // Act
            var val1 = Valizer.Valize(dict);

            //json1 = [{"Key":"Name","Value":"1000 SH 6"},{"Key":"City","Value":"Sugar Land"},{"Key":"State","Value":"TX"},{"Key":"Zip","Value":"77578"},{"Key":"Friends","Value":["Sam","Ana"]}]
            string json1 = val1.ToJson(string.Empty);
            var val2 = Script.Evaluate(json1);
            string json2 = val2.ToJson(string.Empty);

            // Assert
            Assert.AreEqual(json1, json2);
        }
    }
}
