Object DB Mapper
===================================
This projects provides a .NET library that implements a simple Object Relational Mapper based on Microsoft ADO.NET.

## Why another ORM?

The most existing ORM have the problem that they are either large and complex or not flexible enough in database-first scenarios if the database schema varies. 

For example if you have a database of an ERP system that for some customers can have additional columns many ORM can't handle that. Or if you would like to write SQL (e.g. for performance optimization or for manipulating the database's original schema) ORM often have their limits.

Another reason is the license some ORM projects have chosen. For example [ServiceStack.OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite) is a great ORM but it uses the AGPL that is not compatible when using the library in a commercial product.

For that reasons we decided to start our own little Object-to-Database-Mapper.

## What dependencies does this project have?

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