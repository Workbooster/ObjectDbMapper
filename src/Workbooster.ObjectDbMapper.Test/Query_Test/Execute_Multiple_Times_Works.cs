using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Test.Query_Test
{
    [TestFixture]
    public class Execute_Multiple_Times_Works
    {
        class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsMarried { get; set; }
        }

        private DbConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = TestData.SetupConnection();
        }

        [Test]
        public void Iterate_Two_Times_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people WHERE Name = 'Mike'");

                int count = 0;

                foreach (var item in people)
                {
                    count++;
                }

                foreach (var item in people)
                {
                    count++;
                }

                Assert.Greater(count, 0);
            }
        }

        [Test]
        public void Create_Two_Queries_Based_On_The_Same_Connection_Works()
        {
            using (_Connection)
            {
                var p1 = _Connection.Select<Person>(@"SELECT * FROM people");

                int count = 0;

                foreach (var item in p1)
                {
                    count++;
                }

                var p2 = _Connection.Select<Person>(@"SELECT * FROM people WHERE Name = 'Mike'");

                foreach (var item in p1)
                {
                    count++;
                }

                Assert.Greater(count, 0);
            }
        }
    }
}
