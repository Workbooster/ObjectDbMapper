using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Query_Test
{
    [TestFixture]
    public class Reading_To_Properties_Works
    {
        class StringPropertyPerson
        {
            public string Name { get; set; }
        }

        class IntPropertyPerson
        {
            public int Id { get; set; }
        }

        class BoolPropertyPerson
        {
            public bool IsMarried { get; set; }
        }

        class DateTimePropertyPerson
        {
            public DateTime DateOfBirth { get; set; }
        }

        class DateTimeAsStringPropertyPerson
        {
            public string DateOfBirth { get; set; }
        }

        class UnknownPropertyPerson
        {
            public string UnknownOne { get; set; }
            public string UnknownTwo { get; set; }
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
                var people = _Connection.Select<StringPropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC");

                Assert.AreEqual(7, people.Count());
            }
        }

        [Test]
        public void Reading_String_Property_Works()
        {
            using (_Connection)
            {
                IList<StringPropertyPerson> people = _Connection.Select<StringPropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual("Mike", people[0].Name);
                Assert.AreEqual("Larry", people[1].Name);
            }
        }

        [Test]
        public void Reading_Int_Property_Works()
        {
            using (_Connection)
            {
                IList<IntPropertyPerson> people = _Connection.Select<IntPropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual(8, people[0].Id);
                Assert.AreEqual(7, people[1].Id);
            }
        }

        [Test]
        public void Reading_Bool_Property_Works()
        {
            using (_Connection)
            {
                IList<BoolPropertyPerson> people = _Connection.Select<BoolPropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual(true, people[0].IsMarried);
                Assert.AreEqual(false, people[1].IsMarried);
            }
        }

        [Test]
        public void Reading_DateTime_Property_Works()
        {
            using (_Connection)
            {
                IList<DateTimePropertyPerson> people = _Connection.Select<DateTimePropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual(new DateTime(1953, 9, 23), people[0].DateOfBirth);
                Assert.AreEqual(new DateTime(1969, 1, 26), people[1].DateOfBirth);
            }
        }

        [Test]
        public void Reading_DateTime_As_String_Property_Works()
        {
            using (_Connection)
            {
                IList<DateTimeAsStringPropertyPerson> people = _Connection.Select<DateTimeAsStringPropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual(Convert.ToString(new DateTime(1953, 9, 23)), people[0].DateOfBirth);
                Assert.AreEqual(Convert.ToString(new DateTime(1969, 1, 26)), people[1].DateOfBirth);
            }
        }

        [Test]
        public void Reading_Wrong_Property_Throws_FormatException()
        {
            using (_Connection)
            {
                Assert.Throws<FormatException>(delegate
                {
                    IList<DateTimePropertyPerson> people = _Connection.Select<DateTimePropertyPerson>(@"SELECT DateOfBirth = Name FROM people ORDER BY Id DESC").ToList();
                });
            }
        }

        [Test]
        public void Reading_All_Records_With_Only_Unknown_Properties_Works()
        {
            using (_Connection)
            {
                IList<UnknownPropertyPerson> people = _Connection.Select<UnknownPropertyPerson>(@"SELECT * FROM people ORDER BY Id DESC").ToList();

                Assert.AreEqual(7, people.Count);
            }
        }

    }
}