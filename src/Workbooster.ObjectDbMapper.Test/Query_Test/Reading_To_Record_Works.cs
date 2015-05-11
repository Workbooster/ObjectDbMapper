using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Test.Query_Test
{
    [TestFixture]
    public class Reading_To_Record_Works
    {
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
                IList<Record> people = _Connection.Select<Record>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(9, people.Count);
            }
        }

        [Test]
        public void Reading_All_Columns_Works()
        {
            using (_Connection)
            {
                IList<Record> people = _Connection.Select<Record>(@"SELECT * FROM people ORDER BY Id ASC").ToList();

                Assert.AreEqual(5, people[0].Count);
                Assert.AreEqual(4, people[2]["Id"]);
                Assert.AreEqual("Meg", people[2]["Name"]);
                Assert.AreEqual(1, people[2]["IsMarried"]);
                Assert.AreEqual(new DateTime(1965, 3, 9), people[2]["DateOfBirth"]);
                Assert.AreEqual(null, people[2]["PlaceOfBirth"]);
            }
        }

        [Test]
        public void Reading_Records_Filtered_By_Int_Works()
        {
            using (_Connection)
            {
                var people = _Connection.Select<Record>(@"SELECT * FROM people")
                    .Where(new FilterComparison("Id", FilterComparisonOperatorEnum.Equal, 4)).ToList();

                Assert.AreEqual(1, people.Count());
                Assert.AreEqual("Meg", people[0]["Name"]);
            }
        }
    }
}
