# dotnet-6-crud-api

.NET 6.0 - CRUD API Example

Launch API at http://localhost:4000/swagger

Documentation at 
https://jasonwatmore.com/post/2022/03/15/net-6-crud-api-example-and-tutorial

Documentation to add Entity Framework Core at 
https://jasonwatmore.com/post/2022/03/18/net-6-connect-to-sql-server-with-entity-framework-core
- dotnet add package Microsoft.EntityFrameworkCore.SqlServer
- dotnet tool install -g dotnet-ef or dotnet tool update -g dotnet-ef
- dotnet add package Microsoft.EntityFrameworkCore.Design
- dotnet ef migrations add InitialCreate
- dotnet ef database update

/*
 * TODO :
 * create function to wrap all calls to get data from TestData.json
 * add assertions to verify log informations are called
 */
