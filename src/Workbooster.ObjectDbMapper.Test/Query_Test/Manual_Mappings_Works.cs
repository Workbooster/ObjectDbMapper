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
    public class Manual_Mappings_Works
    {
        class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string YearOfBirth { get; set; }
        }

        private DbConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = TestData.SetupConnection();
        }

        [Test]
        public void Overwriting_Auto_FieldMappings_With_Manual_Mappings_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people WHERE Id = 8")
                    .Map("Name", (i, v) => i.Name = v.ToString() + " Blaaa");

                Assert.AreEqual(8, people.First().Id);
                Assert.AreEqual("Mike Blaaa", people.First().Name);
            }
        }

        [Test]
        public void Mapping_Unknown_Fields_With_Manual_Mappings_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people WHERE Id = 8")
                    .Map("DateOfBirth", (i, v) => { i.YearOfBirth = ((DateTime)v).Year.ToString(); });

                Assert.AreEqual(8, people.First().Id);
                Assert.AreEqual("1953", people.First().YearOfBirth);
            }
        }
    }
}
