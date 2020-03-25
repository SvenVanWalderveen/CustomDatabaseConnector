# CustomDatabaseConnector
This DLL is made for connecting to a remote database without writing SQL code in your project.
You only have to add annotations to your classes and properties, and call adapter class methods. 

## Annotations 
First step is adding annotations to the classes which are database table objects. 
Below an example:

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



