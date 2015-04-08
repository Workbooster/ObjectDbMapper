using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Query_Test
{
    [TestFixture]
    public class Reading_To_Fields_With_Column_Attributes_Works
    {
        class StringFieldPerson
        {
            [Column("Name")]
            public string TestName = null;
        }

        class MixedFieldsPerson
        {
            [Column("Id")]
            public int Key = default(int);

            [Column("Name")]
            public string TestName = null;

            public bool IsMarried = default(bool);

            [Column("DateOfBirth")]
            public DateTime Birthday = default(DateTime);

            public string Unknown = null;
        }

        private SqlConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = new SqlConnection(TestData.CONNECTION_STRING);
        }

        [Test]
        public void Reading_String_Field_Works()
        {
            using (_Connection)
            {
                IList<StringFieldPerson> people = _Connection.Select<StringFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual("Mike", people[0].TestName);
                Assert.AreEqual("Larry", people[1].TestName);
            }
        }

        [Test]
        public void Reading_Mixed_Field_Works()
        {
            using (_Connection)
            {
                IList<MixedFieldsPerson> people = _Connection.Select<MixedFieldsPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual("Larry", people[1].TestName);
                Assert.AreEqual(7, people[1].Key);
                Assert.AreEqual(false, people[1].IsMarried);
                Assert.AreEqual(new DateTime(1969, 1, 26), people[1].Birthday);
            }
        }
    }
}
