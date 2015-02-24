using NUnit.Framework;
using Workbooster.ObjectDbMapper.Test._TestData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbooster.ObjectDbMapper.Test.Extensions.DataReaderExtensions_Test
{
    [TestFixture]
    public class Reading_With_Column_Attributes_Works
    {
        class StringPropertyPerson
        {
            [Column("Name")]
            public string TestName { get; set; }
        }

        class MixedPropertiesPerson
        {
            [Column("Id")]
            public int Key { get; set; }

            [Column("Name")]
            public string TestName { get; set; }

            public bool IsMarried { get; set; }

            [Column("DateOfBirth")]
            public DateTime Birthday { get; set; }

            public string Unknown { get; set; }
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
                IList<StringPropertyPerson> people = _Connection.Select<StringPropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual("Mike", people[0].TestName);
                Assert.AreEqual("Larry", people[1].TestName);
            }
        }

        [Test]
        public void Reading_Mixed_Property_Works()
        {
            using (_Connection)
            {
                IList<MixedPropertiesPerson> people = _Connection.Select<MixedPropertiesPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual("Larry", people[1].TestName);
                Assert.AreEqual(7, people[1].Key);
                Assert.AreEqual(false, people[1].IsMarried);
                Assert.AreEqual(new DateTime(1969, 1, 26), people[1].Birthday);
            }
        }
    }
}
