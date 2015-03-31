﻿using System;
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
        static string __Address;
        static VAL __val;

        static ExtensionTest()
        {
            // Arrange
            string code = "{ Name : '1000 SH 6', City : 'Sugar Land', State : 'TX', Zip : '77578'}";
            __val = Script.Evaluate(code);
            __Address = __val.ToString();
        }

        [Fact]
        public void TestToVAL()
        {
            //Arrange
            var address = new { Name="1000 SH 6", City = "Sugar Land", State = "TX", Zip = "77578"};

            // Act
            var val = address.ToVAL();
      

            // Assert
            Assert.Equal(val.ToString(), __Address);
        }

        [Fact]
        public void DictinaryToVAL()
        {
            //Arrange
            var dict = new Dictionary<string, string>();
            dict.Add("Name", "1000 SH 6");
            dict.Add("City",  "Sugar Land");
            dict.Add("State", "TX");
            dict.Add("Zip", "77578");

            // Act
            var val = dict.ToVAL();
            
            // Assert
            Assert.Equal(val.ToString(), __Address);
        }

        [Fact]
        public void EnumerableToVAL()
        {
            //Arrange
            var list = new List<string[]>();
            list.Add(new string[] {"Name", "1000 SH 6"});
            list.Add(new string[] {"City", "Sugar Land"});
            list.Add(new string[] {"State", "TX"});
            list.Add(new string[] {"Zip", "77578"});

            // Act
            var val = list.ToVAL();

            // Assert
            Assert.Equal(val.ToString(), __Address);
        }


        [Fact]
        public void ToDictionary()
        {
            //Arrange
            IDictionary<string, string> dict = __val.ToDictionary<string, string>();

            // Act
            var state = dict["State"];

            // Assert
            Assert.Equal(state, "TX");
        }

        [Fact]
        public void AsNumerable()
        {
            //Arrange
            var list = __val.AsEnumerable<VAL>();

            // Act
            var zip = list.Last().AsEnumerable<string>();

            // Assert
            Assert.Equal(zip.Last(), "77578");
        }
    }
}
