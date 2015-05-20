Object DB Mapper
===================================
This projects provides a .NET library that implements a simple Object Relational Mapper based on Microsoft ADO.NET.

## Usage Examples

This Schema is used in the following examples:

![Example DB Schema](https://raw.githubusercontent.com/Workbooster/ObjectDbMapper/master/doc/img/source/Example-DB-Schema.png) 

### SELECT

You can either use properties or fields in your model:

```csharp
public class Person
{
    public int Id { get; set; }
    public string Name;
}
```

Now we can use the Select<T>(...) extension method on the DbConnection

```csharp
// Write a usual SQL-Statement to select the data
IEnumerable<Person> people = connection
    .Select<Person>(@"SELECT * FROM people WHERE IsMarried = 1");
    
// Result: All married people are loaded
```

Instead of writing a SQL WHERE clause you can use a filter:

```csharp
IEnumerable<Person> people = connection
    .Select<Person>(@"SELECT * FROM people")
    .Where(new FilterComparison("IsMarried", FilterComparisonOperatorEnum.ExactlyEqual, true));
    
// Result: All married people are loaded
```

You are also free to use more complex SQL statements:

```csharp
public class PersonAddressData
{
    public int PersonId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}

public class Example
{
    public void Run(DbConnection connection)
    {
        string largerSql = @"
SELECT 
     PersonId =  p.Id
    ,Name = p.Name
    ,Address = adr.Street + ', ' + adr.City
FROM people AS p
JOIN Addresses AS adr   ON adr.IdPerson = p.Id
WHERE adr.IsPrimary = 1";

        IEnumerable<PersonAddressData> people = connection.Select<PersonAddressData>(largerSql);

        // Do something with the data ...
    }
}

// Result: All people with their primary address are loaded.
```

The only important thing is that the column name returned by the SQL statement matches the name of a public field or property in the model class. Otherwise the data will not be mapped (no error occures!).


### INSERT

Assume we have this model:

```csharp
public class Person
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public bool IsMarried { get; set; }
    public DateTime DateOfBirth { get; set; }
}
```

If the column names of the table and the field/property names of the model match we can use CreateDynamicMappings(...) to map columns and fields/properties:

```csharp
// Prepare a list of people to insert
List<Person> people = new List<Person>() { 
        new Person() { Id = null, Name = "InsertTest-1", IsMarried= false, DateOfBirth = new DateTime(1985, 2,17), },
        new Person() { Id = null, Name = "InsertTest-2", IsMarried= true, DateOfBirth = new DateTime(1972, 11,2), },
        new Person() { Id = null, Name = "InsertTest-3", IsMarried= false, DateOfBirth = new DateTime(1953, 8,15), },
    };

// Specify a list of fields that not should be mapped dynamically
string[] ignoredFieldsForDynamicMapping = new string[] { "Id" };

// Create an insert command for the model "Person" and the table "People" with an automatic field mapping
InsertCommand<Person> cmd = new InsertCommand<Person>(connection, "People");
cmd.CreateDynamicMappings(ignoredFieldsForDynamicMapping);

// Run the insert command for the given list of people
cmd.Execute(people);

// Result: Three people are inserted (with Id=null - what usually triggers an auto increment)
```

It is also possible to manually create a mapping by giving the column name and a lambda expression ("i" is the current object, that will be inserted):

```csharp
cmd.Map("PersonName", i => i.Name);

// Result: The property "Name" is mapped to the database column "PersonName"
```

That also allows you to add some business logic when mapping a field/property to a column:

```csharp
cmd.Map("PersonName", i => {
    if (i.Name == "")
    {
        return "Unknown";
    }
    else
    {
        return i.Name;
    }
});

// Result: The column "PersonName" gets the value "Unknown" if the property "Name" contains an empty string
```

### UPDATE

The Update Command works quite the same way as an Insert Command but it additionally needs a key or a filter used to identify the records in the database.

The following example shows the easiest way to update records identified by one single identifier:

```csharp
Person person = new Person() { Id = 2, Name = "UpdateTest", DateOfBirth = DateTime.Today, };

UpdateCommand<Person> cmd = new UpdateCommand<Person>(connection, "People");
cmd.CreateDynamicMappings();
cmd.MapKey("Id", p => p.Id); // Maps the column "Id" with the property "Id" from the model

cmd.Execute(person);

// Result: The person with Id=2 gets the name "UpdateTest" and the date of today as birthday.
```

Or you can instead use a filter:

```csharp
// Create a "template person" for all people that match the filter criteria
Person person = new Person() { Name = "UpdateTest", IsMarried = true, DateOfBirth = DateTime.Today, };

UpdateCommand<Person> cmd = new UpdateCommand<Person>(connection, "People");
cmd.CreateDynamicMappings();
cmd.RemoveMapping("Id"); // You can also remove mappings created by the dynamic mapping functionality

// Filter: Only people that are married
cmd.Filter = new FilterComparison("IsMarried", FilterComparisonOperatorEnum.ExactlyEqual, true);

cmd.Execute(person);

// Result: The name of all married people is set to "UpdateTest" and their birthday is set to today.
```

And of course you can specify the field mappings by yourself:

```csharp
// Create a "template person"
Person person = new Person() { Name = "UpdateTest", IsMarried = false, DateOfBirth = DateTime.Today, };

UpdateCommand<Person> cmd = new UpdateCommand<Person>(connection, "People");
cmd.Map("IsMarried", i => i.IsMarried);

cmd.Execute(person);

// Result: The column "IsMarried" is set to "false" for all people because there is no filter.
```

### DELETE

Also like with Update Commands you have to specify either a key mapping or and/or a filter condition for deleting records.

The following example shows how to delete a person with a specific filter criteria:

```csharp
DeleteCommand<Person> cmd = new DeleteCommand<Person>(connection, "People");

cmd.Filter = new FilterComparison("IsMarried", FilterComparisonOperatorEnum.ExactlyEqual, true);

cmd.Execute();
// Result: All people that are married are removed.
```

Another way is to specify a key mapping:

```csharp
// prepare people for deletion (only id needed)
IEnumerable<Person> people = new List<Person>() {
    new Person() { Id = 3 },
    new Person() { Id = 4 },
};

DeleteCommand<Person> cmd = new DeleteCommand<Person>(connection, "People");

cmd.MapKey("Id", p => p.Id);

cmd.Execute(people);

// Result: The people with Id 3 and 4 are removed.
```

### Working with Filters

In the examples above filters are only used to specify one criteria. But they can also be used in groups.

```csharp
var people = connection.Select<Person>(@"SELECT * FROM people")
    .Where(FilterGroup.New()
    .Add("Name", FilterComparisonOperatorEnum.Equal, "mike")
    .Add("IsMarried", FilterComparisonOperatorEnum.Equal, false));
```
In MySQL that would result in something like this:

```sql
SELECT sub1.* FROM (SELECT * FROM people) AS sub1 
WHERE ( 
    `Name` LIKE @param1 
AND `IsMarried` LIKE @param2)
```

Or a nested Filter Group:

```csharp
var people = connection.Select<Person>(@"SELECT * FROM people")
    .Where(FilterGroup.New()
    .Add(FilterGroup.New(FilterGroupOperatorEnum.Or)
        .Add("Name", FilterComparisonOperatorEnum.Equal, "mike")
        .Add("Name", FilterComparisonOperatorEnum.Equal, "samuel"))
    .Add("IsMarried", FilterComparisonOperatorEnum.Equal, false));
```
In MySQL that would result in something like this:

```sql
SELECT sub1.* FROM (SELECT * FROM people) AS sub1 
WHERE (
    ( `Name` LIKE @param1 OR `Name` LIKE @param2)
AND `IsMarried` LIKE @param3)
```



## FAQ

### Why another ORM?

The most existing ORM have the problem that they are either large and complex or not flexible enough in database-first scenarios if the database schema varies. 

For example if you have a database of an ERP system that for some customers can have additional columns many ORM can't handle that. Or if you would like to write SQL (e.g. for performance optimization or for manipulating the database's original schema) ORM often have their limits.

Another reason is the license some ORM projects have chosen. For example [ServiceStack.OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite) is a great ORM but it uses the AGPL that is not compatible when using the library in a commercial product.

For that reasons we decided to start our own little Object-to-Database-Mapper.

### What dependencies does this project have?

Non. Only default .NET libraries (e.g. System.Data)

The unit test project (Workbooster.ObjectDbMapper.Test) needs:
* [NUnit - testing framework](http://www.nunit.org/)

## Content

Directory | Description
----------| -------------
/src | the source code (Visual Studio Solution and Projects)
/src/Workbooster.ObjectDbMapper.Test | Some NUnit tests that also show how the library can be used.

## License

This project is licensed under the terms of the MIT License (also known as X11-License). See LICENSE for more information.

## Contact

Workbooster GmbH<br/>
Pfarrain 3a<br/>
8604 Volketswil (Switzerland)<br/>

Web: www.workbooster.ch<br/>
E-Mail: info@workbooster.ch<br/>
Phone: +41 (0)44 515 48 80<br/>