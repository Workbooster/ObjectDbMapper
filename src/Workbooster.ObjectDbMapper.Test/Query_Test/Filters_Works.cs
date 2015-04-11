﻿using NUnit.Framework;
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
    public class Filters_Works
    {
        class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsMarried { get; set; }
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
        public void Reading_Records_Filtered_By_CaseInsensitive_String_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(new FilterComparison("Name", FilterComparisonOperatorEnum.Equal, "mike"));

                Assert.AreEqual(3, people.Count());
            }
        }

        [Test]
        public void Reading_Records_Filtered_By_CaseSensitive_String_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(new FilterComparison("Name", FilterComparisonOperatorEnum.ExactlyEqual, "mike"));

                Assert.AreEqual(1, people.Count());
            }
        }

        [Test]
        public void Reading_Records_Filtered_By_Group_Of_CaseSensitive_Strings_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(FilterGroup.New()
                    .Add("Name", FilterComparisonOperatorEnum.ExactlyEqual, "mike")
                    .Add("Name", FilterComparisonOperatorEnum.ExactlyEqual, "mike"));

                Assert.AreEqual(1, people.Count());
            }
        }

        [Test]
        public void Reading_Records_Filtered_By_Int_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(new FilterComparison("Id", FilterComparisonOperatorEnum.Equal, 8));

                Assert.AreEqual(1, people.Count());
            }
        }

        [Test]
        public void Reading_Records_Filtered_By_Group_Of_String_And_Bool_Values_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(FilterGroup.New()
                    .Add("Name", FilterComparisonOperatorEnum.Equal, "mike")
                    .Add("IsMarried", FilterComparisonOperatorEnum.Equal, false));

                Assert.AreEqual(2, people.Count());
            }
        }

        [Test]
        public void Reading_Records_Filtered_By_Group_Of_LikeString_And_Bool_Values_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Person>(@"SELECT * FROM people")
                    .Where(FilterGroup.New()
                    .Add("Name", FilterComparisonOperatorEnum.Like, "M%")
                    .Add("IsMarried", FilterComparisonOperatorEnum.Equal, true));

                Assert.AreEqual(2, people.Count());
            }
        }
    }
}
