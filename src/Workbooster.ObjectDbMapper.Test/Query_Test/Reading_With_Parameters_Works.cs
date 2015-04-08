using NUnit.Framework;
using Workbooster.ObjectDbMapper.Test._TestData;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper.Query_Test
{
    [TestFixture]
    public class Reading_With_Parameters_Works
    {
        class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private SqlConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = new SqlConnection(TestData.CONNECTION_STRING);
        }

        [Test]
        public void Reading_String_Property_Works()
        {
            using (_Connection)
            {
                string sql = @"SELECT * FROM people WHERE id = @id";
                DbParameter[] parameters = new DbParameter[] { new SqlParameter("@id", 5) };
                IList<Person> people = _Connection.Select<Person>(sql, parameters).ToList();

                Assert.AreEqual(5, people[0].Id);
                Assert.AreEqual("Melanie", people[0].Name);
            }
        }

        [Test]
        public void Reading_String_Property_With_String_Parameter_Works()
        {
            using (_Connection)
            {
                string sql = @"SELECT * FROM people WHERE name = @name";
                DbParameter[] parameters = new DbParameter[] { new SqlParameter("@name", "Melanie") };
                IList<Person> people = _Connection.Select<Person>(sql, parameters).ToList();

                Assert.AreEqual(5, people[0].Id);
                Assert.AreEqual("Melanie", people[0].Name);
            }
        }
    }
}
