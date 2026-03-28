Steps:

1) For the PR.Persistence.EntityFrameworkCore.SqlServer project:
   - Install the nuget package Microsoft.EntityFrameworkCore.Sqlite
   - Install the nuget package Microsoft.EntityFrameworkCore.Tools
   - Create the PersonConfiguration class
   - Create the ConnectionStringProvider class (home made)
   - Create the PRDbContext class
2) Add a DotNet 6.0 Console Application project that references the PR.Persistence.EntityFrameworkCore.Sqlite project
   (in order to generate a migration), and install the nuget package Microsoft.EntityFrameworkCore.Design for it. Also,
   set this project as the Startup project for the solution
3) Open the Package Manager Console window, and set the project PR.Persistence.EntityFrameworkCore.Sqlite as the default
   project
4) In the Package Manager Console window: Execute this line at the prompt:
     add-migration InitialMigration
5) In the Package Manager Console window: Execute this line at the prompt:
     update-database
6) Verify that the database file has been created ... hmm the database is created in the folder of the helper console app
   which is not what we want. We want the database to be created if it doesn't exist (so better leave out step 5)