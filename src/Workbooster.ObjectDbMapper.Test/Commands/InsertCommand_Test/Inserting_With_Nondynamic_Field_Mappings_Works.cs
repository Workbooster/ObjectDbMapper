using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Commands;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Test.Commands.InsertCommand_Test
{
    [TestFixture]
    public class Inserting_With_Nondynamic_Field_Mappings_Works
    {
        class Person
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public bool IsMarried { get; set; }
            public DateTime DateOfBirth { get; set; }
        }

        private SqlConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connection = new SqlConnection(TestData.SetupTempTestDb());
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
        public void Not_Specified_Field_Mappings_Throws_Exception()
        {
            using (_Connection)
            {
                List<Person> people = new List<Person>() { 
                    new Person() { Id = 101, Name = "InsertTest-1", IsMarried= false, DateOfBirth = new DateTime(1985, 2,17) },
                    new Person() { Id = 102, Name = "InsertTest-2", IsMarried= true, DateOfBirth = new DateTime(1972, 11,2) },
                    new Person() { Id = 103, Name = "InsertTest-3", IsMarried= false, DateOfBirth = new DateTime(1953, 8,15) },
                };

                InsertCommand<Person> cmd = new InsertCommand<Person>(_Connection, "People");

                Exception ex = Assert.Throws<Exception>(delegate { cmd.Execute(people); });
                Assert.AreEqual("No field mappings are specified.", ex.Message);
            }
        }

        [Test]
        public void Inserting_Data_Works()
        {
            using (_Connection)
            {
                List<Person> people = new List<Person>() { 
                    new Person() { Id = 101, Name = "InsertTest-1", IsMarried= false, DateOfBirth = new DateTime(1985, 2,17) },
                    new Person() { Id = 102, Name = "InsertTest-2", IsMarried= true, DateOfBirth = new DateTime(1972, 11,2) },
                    new Person() { Id = 103, Name = "InsertTest-3", IsMarried= false, DateOfBirth = new DateTime(1953, 8,15) },
                };

                InsertCommand<Person> cmd = new InsertCommand<Person>(_Connection, "People");
                cmd.Map("Name", p => p.Name);
                cmd.Map("IsMarried", p => p.IsMarried);
                cmd.Map("DateOfBirth", p => p.DateOfBirth);
                cmd.Execute(people);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) 
FROM People 
WHERE (Name = 'InsertTest-1' AND IsMarried = 0) 
OR (Name = 'InsertTest-2' AND IsMarried = 1) 
OR (Name = 'InsertTest-3' AND IsMarried = 0)";

                Assert.AreEqual(3, checkCmd.ExecuteScalar());
            }
        }

        [Test]
        public void Inserting_Large_Data_Works()
        {
            using (_Connection)
            {
                List<Person> people = new List<Person>();

                for (int i = 0; i < 10000; i++)
                {
                    people.Add(new Person() { Id = i + 100, Name = "InsertTest-" + i, IsMarried = false, DateOfBirth = new DateTime(1985, 2, 17) });
                }

                InsertCommand<Person> cmd = new InsertCommand<Person>(_Connection, "People");
                cmd.Map("Name", p => p.Name);
                cmd.Map("IsMarried", p => p.IsMarried);
                cmd.Map("DateOfBirth", p => p.DateOfBirth);
                cmd.Execute(people);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) 
FROM People 
WHERE Name LIKE 'InsertTest%'
";

                Assert.AreEqual(10000, checkCmd.ExecuteScalar());
            }
        }

        [Test]
        public void Using_Insert_Command_Multiple_Times_Works()
        {
            using (_Connection)
            {
                List<Person> people = new List<Person>() { 
                    new Person() { Id = 101, Name = "InsertTest-1", IsMarried= false, DateOfBirth = new DateTime(1985, 2,17) },
                    new Person() { Id = 102, Name = "InsertTest-2", IsMarried= true, DateOfBirth = new DateTime(1972, 11,2) },
                };

                List<Person> people2 = new List<Person>() { 
                    new Person() { Id = 103, Name = "InsertTest-3", IsMarried= true, DateOfBirth = new DateTime(1953, 8,15) },
                    new Person() { Id = 104, Name = "InsertTest-4", IsMarried= false, DateOfBirth = new DateTime(1962, 4,2) },
                };

                InsertCommand<Person> cmd = new InsertCommand<Person>(_Connection, "People");
                cmd.Map("Name", p => p.Name);
                cmd.Map("IsMarried", p => p.IsMarried);
                cmd.Map("DateOfBirth", p => p.DateOfBirth);
                
                // execute with the first collection
                cmd.Execute(people);

                // execute with the second collection
                cmd.Execute(people2);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) 
FROM People 
WHERE (Name = 'InsertTest-1' AND IsMarried = 0) 
OR (Name = 'InsertTest-2' AND IsMarried = 1) 
OR (Name = 'InsertTest-3' AND IsMarried = 1)
OR (Name = 'InsertTest-4' AND IsMarried = 0)";

                Assert.AreEqual(4, checkCmd.ExecuteScalar());
            }
        }
    }
}
