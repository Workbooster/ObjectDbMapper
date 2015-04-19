using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Commands;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Test.Commands.InsertCommand_Test
{
    [TestFixture]
    public class Inserting_With_Dynamic_Field_Mappings_Works
    {
        class Person
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public bool IsMarried { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string PlaceOfBirth { get; set; }
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
        public void Inserting_Data_Works()
        {
            using (_Connection)
            {
                List<Person> people = new List<Person>() { 
                    new Person() { Id = 101, Name = "InsertTest-1", IsMarried= false, DateOfBirth = new DateTime(1985, 2,17), PlaceOfBirth = "Zurich", },
                    new Person() { Id = 102, Name = "InsertTest-2", IsMarried= true, DateOfBirth = new DateTime(1972, 11,2), PlaceOfBirth = "London", },
                    new Person() { Id = 103, Name = "InsertTest-3", IsMarried= false, DateOfBirth = new DateTime(1953, 8,15), PlaceOfBirth = "New York", },
                };

                InsertCommand<Person> cmd = new InsertCommand<Person>(_Connection, "People");
                cmd.CreateDynamicMappings(new string[] { "Id" });
                cmd.Execute(people);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) 
FROM People 
WHERE (Name = 'InsertTest-1' AND IsMarried = 0 AND PlaceOfBirth = 'Zurich') 
OR (Name = 'InsertTest-2' AND IsMarried = 1 AND PlaceOfBirth = 'London') 
OR (Name = 'InsertTest-3' AND IsMarried = 0 AND PlaceOfBirth = 'New York')";

                Assert.AreEqual(3, checkCmd.ExecuteScalar());
            }
        }

        [Test]
        public void Removing_A_Column_Mapping_Works()
        {
            using (_Connection)
            {
                List<Person> people = new List<Person>() { 
                    new Person() { Id = 101, Name = "InsertTest-1", IsMarried= false, DateOfBirth = new DateTime(1985, 2,17), PlaceOfBirth = "Zurich", },
                    new Person() { Id = 102, Name = "InsertTest-2", IsMarried= true, DateOfBirth = new DateTime(1972, 11,2), PlaceOfBirth = "London", },
                    new Person() { Id = 103, Name = "InsertTest-3", IsMarried= false, DateOfBirth = new DateTime(1953, 8,15), PlaceOfBirth = "New York", },
                };

                InsertCommand<Person> cmd = new InsertCommand<Person>(_Connection, "People");
                cmd.CreateDynamicMappings(new string[] { "Id" });
                cmd.RemoveMapping("PlaceOfBirth");
                cmd.Execute(people);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) 
FROM People 
WHERE (Name = 'InsertTest-1' AND IsMarried = 0 AND PlaceOfBirth IS NULL) 
OR (Name = 'InsertTest-2' AND IsMarried = 1 AND PlaceOfBirth IS NULL) 
OR (Name = 'InsertTest-3' AND IsMarried = 0 AND PlaceOfBirth IS NULL)";

                Assert.AreEqual(3, checkCmd.ExecuteScalar());
            }
        }

        [Test]
        public void Overwriting_A_Column_Mapping_Works()
        {
            using (_Connection)
            {
                List<Person> people = new List<Person>() { 
                    new Person() { Id = 101, Name = "InsertTest-1", IsMarried= false, DateOfBirth = new DateTime(1985, 2,17), PlaceOfBirth = "Zurich", },
                    new Person() { Id = 102, Name = "InsertTest-2", IsMarried= true, DateOfBirth = new DateTime(1972, 11,2), PlaceOfBirth = "London", },
                    new Person() { Id = 103, Name = "InsertTest-3", IsMarried= false, DateOfBirth = new DateTime(1953, 8,15), PlaceOfBirth = "New York", },
                };

                InsertCommand<Person> cmd = new InsertCommand<Person>(_Connection, "People");
                cmd.CreateDynamicMappings(new string[] { "Id" });
                cmd.Map("PlaceOfBirth", o => "Unknown");
                cmd.Execute(people);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) 
FROM People 
WHERE (Name = 'InsertTest-1' AND IsMarried = 0 AND PlaceOfBirth = 'Unknown') 
OR (Name = 'InsertTest-2' AND IsMarried = 1 AND PlaceOfBirth = 'Unknown') 
OR (Name = 'InsertTest-3' AND IsMarried = 0 AND PlaceOfBirth = 'Unknown')";

                Assert.AreEqual(3, checkCmd.ExecuteScalar());
            }
        }
    }
}
