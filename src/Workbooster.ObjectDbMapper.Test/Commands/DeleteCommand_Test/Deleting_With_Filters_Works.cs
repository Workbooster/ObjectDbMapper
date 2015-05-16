using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Commands;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Test.Commands.DeleteCommand_Test
{
    [TestFixture]
    public class Deleting_With_Filters_Works
    {
        class Person
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public bool IsMarried { get; set; }
            public DateTime DateOfBirth { get; set; }
        }

        private DbConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = TestData.SetupConnection();
        }

        [TearDown]
        public void TearDown()
        {
            if (_Connection.State != System.Data.ConnectionState.Closed)
            {
                _Connection.Close();
            }
        }

        [Test]
        public void Deleting_With_Filter_Condition()
        {
            using (_Connection)
            {
                // get expected value
                var expectedCmd = _Connection.CreateCommand();
                expectedCmd.CommandText = @"SELECT COUNT(*) FROM People";

                int numberOfPeopleBeforeDeletion = Convert.ToInt32(expectedCmd.ExecuteScalar());

                if (numberOfPeopleBeforeDeletion == 0) throw new Exception("No people found");

                DeleteCommand<Person> cmd = new DeleteCommand<Person>(_Connection, "People");

                cmd.Filter = new FilterComparison("Id", FilterComparisonOperatorEnum.ExactlyEqual, 4);

                cmd.Execute();

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"SELECT COUNT(*) FROM People";

                int numberOfPeopleAfterDeletion = Convert.ToInt32(checkCmd.ExecuteScalar());

                Assert.AreEqual(numberOfPeopleBeforeDeletion - 1, numberOfPeopleAfterDeletion);
            }
        }

        [Test]
        public void Deleting_With_Key_Mappings_Condition()
        {
            using (_Connection)
            {
                // get expected value
                var expectedCmd = _Connection.CreateCommand();
                expectedCmd.CommandText = @"SELECT COUNT(*) FROM People";

                int numberOfPeopleBeforeDeletion = Convert.ToInt32(expectedCmd.ExecuteScalar());

                if (numberOfPeopleBeforeDeletion == 0) throw new Exception("No people found");

                // prepare people for deletion (only id needed)
                IEnumerable<Person> people = new List<Person>() {
                    new Person() { Id = 3 },
                    new Person() { Id = 4 },
                };

                DeleteCommand<Person> cmd = new DeleteCommand<Person>(_Connection, "People");

                cmd.MapKey("Id", p => p.Id);

                cmd.Execute(people);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"SELECT COUNT(*) FROM People";

                int numberOfPeopleAfterDeletion = Convert.ToInt32(checkCmd.ExecuteScalar());

                Assert.AreEqual(numberOfPeopleBeforeDeletion - 2, numberOfPeopleAfterDeletion);
            }
        }
    }
}
