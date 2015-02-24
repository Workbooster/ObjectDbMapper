using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Test.Extensions.DataReaderExtensions_Test
{
    [TestFixture]
    public class Reading_To_Fields_Works
    {
        class StringFieldPerson
        {
            public string Name = null;
        }

        class IntFieldPerson
        {
            public int Id = default(int);
        }

        class BoolFieldPerson
        {
            public bool IsMarried = default(bool);
        }

        class DateTimeFieldPerson
        {
            public DateTime DateOfBirth = default(DateTime);
        }

        class DateTimeAsStringFieldPerson
        {
            public string DateOfBirth = null;
        }

        class UnknownFieldPerson
        {
            public string UnknownOne = null;
            public string UnknownTwo = null;
        }

        private SqlConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = new SqlConnection(TestData.CONNECTION_STRING);
        }

        [Test]
        public void Reading_All_Records_Works()
        {
            using (_Connection)
            {
                IList<StringFieldPerson> people = _Connection.Select<StringFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual(7, people.Count);
            }
        }

        [Test]
        public void Reading_String_Field_Works()
        {
            using (_Connection)
            {
                IList<StringFieldPerson> people = _Connection.Select<StringFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual("Mike", people[0].Name);
                Assert.AreEqual("Larry", people[1].Name);
            }
        }

        [Test]
        public void Reading_Int_Field_Works()
        {
            using (_Connection)
            {
                IList<IntFieldPerson> people = _Connection.Select<IntFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual(8, people[0].Id);
                Assert.AreEqual(7, people[1].Id);
            }
        }

        [Test]
        public void Reading_Bool_Field_Works()
        {
            using (_Connection)
            {
                IList<BoolFieldPerson> people = _Connection.Select<BoolFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual(true, people[0].IsMarried);
                Assert.AreEqual(false, people[1].IsMarried);
            }
        }

        [Test]
        public void Reading_DateTime_Field_Works()
        {
            using (_Connection)
            {
                IList<DateTimeFieldPerson> people = _Connection.Select<DateTimeFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual(new DateTime(1953, 9, 23), people[0].DateOfBirth);
                Assert.AreEqual(new DateTime(1969, 1, 26), people[1].DateOfBirth);
            }
        }

        [Test]
        public void Reading_DateTime_As_String_Field_Works()
        {
            using (_Connection)
            {
                IList<DateTimeAsStringFieldPerson> people = _Connection.Select<DateTimeAsStringFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual(Convert.ToString(new DateTime(1953, 9, 23)), people[0].DateOfBirth);
                Assert.AreEqual(Convert.ToString(new DateTime(1969, 1, 26)), people[1].DateOfBirth);
            }
        }

        [Test]
        public void Reading_Wrong_Field_Throws_FormatException()
        {
            using (_Connection)
            {
                Assert.Throws<FormatException>(delegate
                {
                    IList<DateTimeFieldPerson> people = _Connection.Select<DateTimeFieldPerson>(@"SELECT DateOfBirth = Name FROM people ORDER BY Id DESC");
                });
            }
        }

        [Test]
        public void Reading_All_Records_With_Only_Unknown_Properties_Works()
        {
            using (_Connection)
            {
                IList<UnknownFieldPerson> people = _Connection.Select<UnknownFieldPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual(7, people.Count);
            }
        }
    }
}
