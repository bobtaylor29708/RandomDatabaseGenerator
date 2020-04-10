# README

# Random Database Generator

### Purpose
Random database generator is a command line utility that will generate a database with a 
random number of tables, each table having a random number of columns and a random number 
of rows per column.

## Setup the destination database
Make sure you set the DataSource if you are not using localhost (default instance)

```Csharp 
  <connectionStrings>
    <!-- Be sure to set the SQL Instance below if not using the localhost-->
    <add name="Destination" providerName="Sql" connectionString="Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;"/>
  </connectionStrings>
```
Additional configuration items you should set:
```Csharp   
<!-- Set this to name the database you are creating -->
<add key="DatabaseName" value="RandomDB"/> 
```

Here is where you specify the number of tables, and the limits for number of columns
per table and number of rows per table
```Csharp 
<add key="NumberOfTablesToCreate" value="10"/>
<!-- Set this to the maximum number of columns per table (randomly generated per table) -->
<add key="MaxNumberOfColumnsPerTable" value="20"/>
<!-- Set this to the maximum number of rows per table (randomly generated per table) -->
<add key="MaxNumberOfRowsPerTable" value ="10"/>
```

This entry allows you to set a folder where activity.log gets written as well as the 
SQL script if you choose to write that as well

```Csharp 
<!-- Set this to a location were you would like the Generated SQL Script and log files to be written -->
<add key="ScriptFolderPath" value="C:\temp\"/>
```

And finally you can turn on or turn off the SQL Script generation. It is on by default.
This way you can generate the file and use it on multiple systems.

```Csharp 
<!-- Set this to true to write the SQL Statements to create the tables and insert the data -->
<add key="CreateSQLScript" value="true"/>
```
  
