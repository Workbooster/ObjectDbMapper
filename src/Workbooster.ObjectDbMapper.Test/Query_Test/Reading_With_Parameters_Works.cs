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

        private DbConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = TestData.SetupConnection();
        }

        [Test]
        public void Reading_String_Property_Works()
        {
            using (_Connection)
            {
                DbCommand cmd = _Connection.CreateCommand();
                DbParameter param = cmd.CreateParameter();
                param.ParameterName = "@id";
                param.Value = 5;
                DbParameter[] parameters = new DbParameter[] { param };

                string sql = @"SELECT * FROM people WHERE id = @id";
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
                DbCommand cmd = _Connection.CreateCommand();
                DbParameter param = cmd.CreateParameter();
                param.ParameterName = "@name";
                param.Value = "Melanie";
                DbParameter[] parameters = new DbParameter[] { param };

                string sql = @"SELECT * FROM people WHERE name = @name";
                IList<Person> people = _Connection.Select<Person>(sql, parameters).ToList();

                Assert.AreEqual(5, people[0].Id);
                Assert.AreEqual("Melanie", people[0].Name);
            }
        }
    }
}
