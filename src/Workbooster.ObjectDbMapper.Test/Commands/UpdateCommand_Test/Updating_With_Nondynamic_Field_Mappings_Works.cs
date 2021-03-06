﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Commands;
using Workbooster.ObjectDbMapper.Test._TestData;

namespace Workbooster.ObjectDbMapper.Test.Commands.UpdateCommand_Test
{
    [TestFixture]
    public class Updating_With_Nondynamic_Field_Mappings_Works
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
        public void Updating_Data_Without_Filter_Works()
        {
            using (_Connection)
            {
                Person person = new Person() { Name = "UpdateTest", };

                UpdateCommand<Person> cmd = new UpdateCommand<Person>(_Connection, "People");
                cmd.Map("Name", p => p.Name);
                cmd.Execute(person);

                // get expected value
                var expectedCmd = _Connection.CreateCommand();
                expectedCmd.CommandText = @"
SELECT COUNT(*) FROM People";

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) FROM People WHERE Name = 'UpdateTest'";

                Assert.AreEqual(expectedCmd.ExecuteScalar(), checkCmd.ExecuteScalar());
            }
        }

        [Test]
        public void Updating_Data_With_Key_Filter_Works()
        {
            using (_Connection)
            {
                Person person = new Person() { Id = 2, Name = "UpdateTest", };

                UpdateCommand<Person> cmd = new UpdateCommand<Person>(_Connection, "People");
                cmd.Map("Name", p => p.Name);
                cmd.MapKey("Id", p => p.Id);
                cmd.Execute(person);

                // check
                var checkCmd = _Connection.CreateCommand();
                checkCmd.CommandText = @"
SELECT COUNT(*) FROM People WHERE Name = 'UpdateTest' AND Id = 2";

                Assert.AreEqual(1, checkCmd.ExecuteScalar());
            }
        }
    }
}
