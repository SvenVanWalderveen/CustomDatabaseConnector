# CustomDatabaseConnector
This DLL is made for connecting to a remote database without writing SQL code in your project.
You only have to add annotations to your classes and properties, and call adapter class methods. 

## Table of contents
* [Start-up](#startup)
* [Annotations](#annotations)
* [Adapter methods](#adapter-methods)

## Start-up
First step is to make sure you added this DLL to your project.

## Annotations 
First step is adding annotations to the classes which are database table objects. 
Two types of annotations are available:
* **CustomDatabaseClassAnnotation**: Annotation for a .NET class
* **CustomDatabaseColumnAnnotation**: Annotation for a .NET property

**CustomDatabaseClassAnnotation**
This annotation class is used for determing the table name. 

This annotation class has the following properties:
* **TableName** (string): Tablename of database table.

**CustomDatabaseColumnAnnotation**
This annotation class is used for setting properties of table columns.

This annotation class has the following properties:
* **IsAutoIncrement** (bool): Set if property is an auto-increment field. Default value: false
* **IsPrimaryKey** (bool): Set if property is Primary Key. Default value: false
* **ColumnName** (string): To set the name of the column used in remote table. Default value: null
* **ForeignKeyTable** (Type): To set reference to another table. Setting class type is enough. It will automatically determine the primary key of that table. Default value: null
* **IsNullable** (bool): To set if property can be nullable. Default value: false
* **IsUpdatable** (bool): This property is used for generating UPDATE command. If value is true, then column value will be updated. Default value: false
* **MaxLength** (int): Set max length of field. Default value: 0. 0 means MaxLength isn't used for generating table definition.

Below an example of using the annotations:

```
[CustomDatabaseClassAnnotation(TableName = "TESTTABLE")]
public class TestTable
{
	[CustomDatabaseColumnAnnotation(ColumnName = "ID", IsPrimaryKey = true, IsAutoIncrement = true, IsNullable = false)]
	public int Id { get; set; }
	[CustomDatabaseColumnAnnotation(ColumnName = "NAME", IsNullable = false, IsUpdatable = true, MaxLength = 100)]
	public string Name { get; set; }
	[CustomDatabaseColumnAnnotation(ColumnName = "TEST2", IsNullable = false, IsUpdatable = true)]
	public string Test { get; set; }
}
```

## Adapter methods


