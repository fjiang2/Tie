using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Tie;
using Tie.Helper;
using Xunit;


namespace Tie.Helper.Tests
{
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

        [Fact]
        public void GetValueString2()
        {
            string host = (string)config.Cassandra.Host;
            
            // Assert
            Assert.Equal(host, "192.168.0.20");
        }

        [Fact]
        public void GetValueString3()
        {
            string db = (string)config.Cassandra.Session.db;

            // Assert
            Assert.Equal(db, "database");
        }

        [Fact]
        public void GetValueInteger()
        {
            int port = (int)config.Cassandra.Port;

            // Assert
            Assert.Equal(port, 12345);
        }

        [Fact]
        public void GetValueDouble()
        {
            double d = (double)config.Max;

            // Assert
            Assert.Equal(d, 2.3);
        }

        [Fact]
        public void GetValueBoolean()
        {
            bool b = (bool)config.IsWindow7;

            // Assert
            Assert.Equal(b, true);
        }

        [Fact]
        public void GetValueDateTime()
        {
            DateTime time = (DateTime)config.date;

            // Assert
            Assert.Equal(time, new DateTime(2015,2,3));
        }

        [Fact]
        public void SetValueString()
        {
            config.TempDirectory = "c:\\temp";
            Assert.Equal(DS["TempDirectory"].HostValue, "c:\\temp");

            string temp = (string)config.TempDirectory;
            Assert.Equal(temp, "c:\\temp");
        }

        [Fact]
        public void SetValueString2()
        {
            config.Directory.Temp = "c:\\temp";
            Assert.Equal(DS["Directory"]["Temp"].HostValue, "c:\\temp");

            string temp = (string)config.Directory.Temp;
            Assert.Equal(temp, "c:\\temp");
        }

        [Fact]
        public void SetValueInt3()
        {
            config.A.B.C = 12;
            Assert.Equal(DS["A"]["B"]["C"].HostValue, 12);

            int temp = (int)config.A.B.C;
            Assert.Equal(temp, 12);
        }
    }
}
