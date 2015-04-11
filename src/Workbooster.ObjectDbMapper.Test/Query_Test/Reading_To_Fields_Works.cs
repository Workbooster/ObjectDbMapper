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

        private string _TempDbConnectionString;
        private SqlConnection _Connection;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _TempDbConnectionString = TestData.SetupTempTestDb();
        }

        [SetUp]
        public void Setup()
        {
            _Connection = new SqlConnection(_TempDbConnectionString);
        }

        [Test]
        public void Reading_All_Records_Works()
        {
            using (_Connection)
            {
                IList<StringFieldPerson> people = _Connection.Select<StringFieldPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(9, people.Count);
            }
        }

        [Test]
        public void Reading_String_Field_Works()
        {
            using (_Connection)
            {
                IList<StringFieldPerson> people = _Connection.Select<StringFieldPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual("Mike", people[0].Name);
                Assert.AreEqual("Steve", people[1].Name);
            }
        }

        [Test]
        public void Reading_Int_Field_Works()
        {
            using (_Connection)
            {
                IList<IntFieldPerson> people = _Connection.Select<IntFieldPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(2, people[0].Id);
                Assert.AreEqual(3, people[1].Id);
            }
        }

        [Test]
        public void Reading_Bool_Field_Works()
        {
            using (_Connection)
            {
                IList<BoolFieldPerson> people = _Connection.Select<BoolFieldPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(false, people[0].IsMarried);
                Assert.AreEqual(true, people[2].IsMarried);
            }
        }

        [Test]
        public void Reading_DateTime_Field_Works()
        {
            using (_Connection)
            {
                IList<DateTimeFieldPerson> people = _Connection.Select<DateTimeFieldPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(new DateTime(1985, 6, 13), people[0].DateOfBirth);
                Assert.AreEqual(new DateTime(1978, 2, 3), people[1].DateOfBirth);
            }
        }

        [Test]
        public void Reading_DateTime_As_String_Field_Works()
        {
            using (_Connection)
            {
                IList<DateTimeAsStringFieldPerson> people = _Connection.Select<DateTimeAsStringFieldPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(Convert.ToString(new DateTime(1985, 6, 13)), people[0].DateOfBirth);
                Assert.AreEqual(Convert.ToString(new DateTime(1978, 2, 3)), people[1].DateOfBirth);
            }
        }

        //[Test]
        public void Reading_Wrong_Field_Throws_FormatException()
        {
            using (_Connection)
            {
                Assert.Throws<FormatException>(delegate
                {
                    IList<DateTimeFieldPerson> people = _Connection.Select<DateTimeFieldPerson>(@"SELECT DateOfBirth = Name FROM people ORDER BY Id ASC").ToList();
                });
            }
        }

        [Test]
        public void Reading_All_Records_With_Only_Unknown_Properties_Works()
        {
            using (_Connection)
            {
                IList<UnknownFieldPerson> people = _Connection.Select<UnknownFieldPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(9, people.Count);
            }
        }
    }
}
