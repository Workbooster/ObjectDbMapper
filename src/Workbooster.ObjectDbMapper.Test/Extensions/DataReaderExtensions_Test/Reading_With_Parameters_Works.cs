using NUnit.Framework;
using Workbooster.ObjectDbMapper.Test._TestData;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper.Test.Extensions.DataReaderExtensions_Test
{
    [TestFixture]
    public class Reading_With_Parameters_Works
    {
        class StringPropertyPerson
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
                IList<StringPropertyPerson> people = _Connection.Select<StringPropertyPerson>(sql, parameters);
                
                Assert.AreEqual(5, people[0].Id);
                Assert.AreEqual("Melanie", people[0].Name);
            }
        }
    }
}
