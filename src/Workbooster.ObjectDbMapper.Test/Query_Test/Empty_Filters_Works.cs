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
    public class Empty_Filters_Works
    {
        class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsMarried { get; set; }
        }

        private DbConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = TestData.SetupConnection();
        }

        [Test]
        public void Using_Empty_FilterGroup_Works()
        {
            using (_Connection)
            {
                FilterGroup filter = new FilterGroup();

                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(filter);

                Assert.AreEqual(9, people.Count());
            }
        }

        [Test]
        public void Using_Empty_FilterComparison_Works()
        {
            using (_Connection)
            {
                var filter = new FilterComparison();

                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(filter);

                Assert.AreEqual(9, people.Count());
            }
        }

        [Test]
        public void Using_Empty_Sub_FilterGroup_Works()
        {
            using (_Connection)
            {

                FilterGroup filter = FilterGroup.New()
                    .Add("Name", FilterComparisonOperatorEnum.ExactlyEqual, "mike")
                    .Add(new FilterGroup())
                    .Add("Name", FilterComparisonOperatorEnum.ExactlyEqual, "mike");

                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(filter);

                Assert.AreEqual(1, people.Count());
            }
        }

        [Test]
        public void Using_Empty_Sub_FilterGroup_As_First_Sub_Item_Works()
        {
            using (_Connection)
            {

                FilterGroup filter = FilterGroup.New()
                    .Add(new FilterGroup())
                    .Add("Name", FilterComparisonOperatorEnum.ExactlyEqual, "mike")
                    .Add("Name", FilterComparisonOperatorEnum.ExactlyEqual, "mike");

                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(filter);

                Assert.AreEqual(1, people.Count());
            }
        }

    }
}
