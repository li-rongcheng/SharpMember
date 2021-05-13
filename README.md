# Notes for SharpMember

## Database

- Support SQL Server, PostgreSQL and SQLite

- Multiple provider migration is suported in the following two projects:
  - SharpMember.Migrations.Postgres
  - SharpMember.Migrations.SqlServer

- Database type is specified in:
  
  `GlobalConfigs.DatabaseType`

- Database is configured in:
  
  `SharpMember\SharpMember.Core\ServiceExt.cs`

- Connection Info
  - SQLite file name is in "DbConsts.SqliteDbFileName"
  - For SQL Server and PostgreSQL, see the setting file:

    SharpMember/appsettings.json

- Database migration:
  - Change the current database type in:
    `SharpMember\SharpMember.Core\Definitions\GlobalConfigs.cs`

  - Navigate to project `SharpMember.Migrations.Postgres` or
    `SharpMember.Migrations.SqlServer`, do:

    > add-migration <migration-name>

    then apply by:

    > update-database <migration-name>

## Create Database

- Run postgres docker container by executing `docker-compose up` in the solution folder
 
- Change the current database type in:
  `SharpMember\SharpMember.Core\Definitions\GlobalConfigs.cs`

- Uncomment the following section in `SharpMember/appsettings.json`:
  > "UnitTestConnectionEnabled": true,

- Navigate to project `SharpMember.Migrations.Postgres` or
  `SharpMember.Migrations.SqlServer`, execute `update-database.bat`

