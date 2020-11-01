using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Tie;
using Tie.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Tie.Helper.Tests
{
    [TestClass]
    public class DbMemoryFacts
    {
        static Memory DS = new Memory();
        static dynamic config;

        static DbMemoryFacts()
        {
            string code = @"
                Cassandra.Host	='192.168.0.20';
                Cassandra.Password	='password';
                Cassandra.Port=	12345;
                Cassandra.Session.db =	'database';
                Cassandra.UserName=	'sa';
                Max = 2.3;
                IsWindow7 = true;
                date = DateTime(2015,2,3);
        ";

            Script.Execute(code, DS);
            config = new ApplicationMemoryTest(DS);
            config.Load();
        }

        [TestMethod]
        public void GetValueString2()
        {
            string host = (string)config.Cassandra.Host;
            
            // Assert
            Assert.AreEqual(host, "192.168.0.20");
        }

        [TestMethod]
        public void GetValueString3()
        {
            string db = (string)config.Cassandra.Session.db;

            // Assert
            Assert.AreEqual(db, "database");
        }

        [TestMethod]
        public void GetValueInteger()
        {
            int port = (int)config.Cassandra.Port;

            // Assert
            Assert.AreEqual(port, 12345);
        }

        [TestMethod]
        public void GetValueDouble()
        {
            double d = (double)config.Max;

            // Assert
            Assert.AreEqual(d, 2.3);
        }

        [TestMethod]
        public void GetValueBoolean()
        {
            bool b = (bool)config.IsWindow7;

            // Assert
            Assert.AreEqual(b, true);
        }

        [TestMethod]
        public void GetValueDateTime()
        {
            DateTime time = (DateTime)config.date;

            // Assert
            Assert.AreEqual(time, new DateTime(2015,2,3));
        }

        [TestMethod]
        public void GetValueUndefinedKey()
        {
            dynamic obj = config.UndefinedKey;

            // Assert
            Assert.AreEqual(obj, config.Empty);
        }

        [TestMethod]
        public void CompareUndefinedValue()
        {
            dynamic obj = config.UndefinedKey;

            bool result = obj == config.Empty;

            // Assert
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void GetValueUndefinedKey2()
        {
            object obj = config.UndefinedKey.A;

            // Assert
            Assert.AreEqual(obj, config.Empty);
        }

        [TestMethod]
        public void SetValueString()
        {
            config.TempDirectory = "c:\\temp";
            Assert.AreEqual(DS["TempDirectory"].HostValue, "c:\\temp");

            string temp = (string)config.TempDirectory;
            Assert.AreEqual(temp, "c:\\temp");
        }

        [TestMethod]
        public void SetValueString2()
        {
            config.Directory.Temp = "c:\\temp";
            Assert.AreEqual(DS["Directory"]["Temp"].HostValue, "c:\\temp");

            string temp = (string)config.Directory.Temp;
            Assert.AreEqual(temp, "c:\\temp");
        }

        [TestMethod]
        public void SetValueInt3()
        {
            config.A.B.C = 12;
            Assert.AreEqual(DS["A"]["B"]["C"].HostValue, 12);

            int temp = (int)config.A.B.C;
            Assert.AreEqual(temp, 12);
        }
    }
}
