Steps:

1) For the PR.Persistence.EntityFrameworkCore.SqlServer project:
   - Install the nuget package Microsoft.EntityFrameworkCore.SqlServer
   - Install the nuget package Microsoft.EntityFrameworkCore.Tools
   - Create the PersonConfiguration class
   - Create the ConnectionStringProvider class (home made)
   - Create the PRDbContext class
2) Add a DotNet 6.0 Console Application project that references the PR.Persistence.EntityFrameworkCore.SqlServer project
   (in order to generate a migration), and install the nuget package Microsoft.EntityFrameworkCore.Design for it. Also,
   set this project as the Startup project for the solution
3) Open the Package Manager Console window, and set the project PR.Persistence.EntityFrameworkCore.SqlServer as the default
   project
4) In the Package Manager Console window: Execute this line at the prompt:
     add-migration InitialMigration
5) In the Package Manager Console window: Execute this line at the prompt:
     update-database
6) Use Sql Server Management Studio to verify that the database has been created

Notice:
* Apparently you dont need the DbConfigurationType construct you used for the EntityFramework (i.e. not Core) project
* You dont need to execute enable-migrations like you did before
* Go to the Udemy course "Entity Framework Core - A Full Tour" by Trevoir Williams to see the procedure