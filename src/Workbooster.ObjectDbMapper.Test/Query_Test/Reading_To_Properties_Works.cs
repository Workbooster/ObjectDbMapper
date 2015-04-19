using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        private DbConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = TestData.SetupConnection();
        }

        [Test]
        public void Reading_All_Records_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<StringPropertyPerson>(@"SELECT * FROM people ORDER BY Id ASC");

                Assert.AreEqual(9, people.Count());
            }
        }

        [Test]
        public void Reading_String_Property_Works()
        {
            using (_Connection)
            {
                IList<StringPropertyPerson> people = _Connection.Select<StringPropertyPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual("Mike", people[0].Name);
                Assert.AreEqual("Steve", people[1].Name);
            }
        }

        [Test]
        public void Reading_Int_Property_Works()
        {
            using (_Connection)
            {
                IList<IntPropertyPerson> people = _Connection.Select<IntPropertyPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(2, people[0].Id);
                Assert.AreEqual(3, people[1].Id);
            }
        }

        [Test]
        public void Reading_Bool_Property_Works()
        {
            using (_Connection)
            {
                IList<BoolPropertyPerson> people = _Connection.Select<BoolPropertyPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(false, people[0].IsMarried);
                Assert.AreEqual(true, people[2].IsMarried);
            }
        }

        [Test]
        public void Reading_DateTime_Property_Works()
        {
            using (_Connection)
            {
                IList<DateTimePropertyPerson> people = _Connection.Select<DateTimePropertyPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(new DateTime(1985, 6, 13), people[0].DateOfBirth);
                Assert.AreEqual(new DateTime(1978, 2, 3), people[1].DateOfBirth);
            }
        }

        [Test]
        public void Reading_DateTime_As_String_Property_Works()
        {
            using (_Connection)
            {
                IList<DateTimeAsStringPropertyPerson> people = _Connection.Select<DateTimeAsStringPropertyPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(Convert.ToString(new DateTime(1985, 6, 13)), people[0].DateOfBirth);
                Assert.AreEqual(Convert.ToString(new DateTime(1978, 2, 3)), people[1].DateOfBirth);
            }
        }

        //[Test]
        public void Reading_Wrong_Property_Throws_FormatException()
        {
            using (_Connection)
            {
                Assert.Throws<FormatException>(delegate
                {
                    IList<DateTimePropertyPerson> people = _Connection.Select<DateTimePropertyPerson>(@"SELECT DateOfBirth = Name FROM people ORDER BY Id ASC").ToList();
                });
            }
        }

        [Test]
        public void Reading_All_Records_With_Only_Unknown_Properties_Works()
        {
            using (_Connection)
            {
                IList<UnknownPropertyPerson> people = _Connection.Select<UnknownPropertyPerson>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(9, people.Count);
            }
        }

    }
}