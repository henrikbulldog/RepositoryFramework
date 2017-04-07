using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.Test.Filters;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace RepositoryFramework.Dapper.Test
{
  public class StoredProcedureRepositoryTest : RepositoryTest
  {
    protected override IDbConnection CreateConnection()
    {
      //return new SqlConnection(@"Server=.\SQLEXPRESS;Database=StoredProcedureRepositoryTest;Trusted_Connection=True;");
      return new SqlConnection("Server=BL-FA-STG01;Database=HTH;Trusted_Connection=True;");
    }

    private static void RunScript(IDbConnection connection, string script)
    {
      Regex regex = new Regex(@"\r{0,1}\nGO\r{0,1}\n");
      string[] commands = regex.Split(script);

      for (int i = 0; i < commands.Length; i++)
      {
        if (commands[i] != string.Empty)
        {
          using (var command = connection.CreateCommand())
          {
            command.CommandText = commands[i];
            command.ExecuteNonQuery();
          }
        }
      }
    }

    protected override void InitializeDatabase(IDbConnection connection)
    {
      if (connection.State != ConnectionState.Open)
      {
        connection.Open();
      }

      var script = $@"
IF OBJECT_ID ('Category') IS NOT NULL 
  DROP TABLE Category
GO

CREATE TABLE Category (
  Id INTEGER IDENTITY,
  Name NVARCHAR(100),
  Description NVARCHAR(100)
)
GO

IF OBJECT_ID ('CreateCategory') IS NOT NULL 
  DROP PROCEDURE CreateCategory
GO

CREATE PROCEDURE CreateCategory
  @Name NVARCHAR(100) = NULL,
  @Description NVARCHAR(100) = NULL
AS
BEGIN
  INSERT INTO Category (Name, Description)
  VALUES(@Name, @Description)

  SELECT @@IDENTITY
END
GO

IF OBJECT_ID ('GetCategory') IS NOT NULL 
  DROP PROCEDURE GetCategory
GO
  
CREATE PROCEDURE GetCategory
  @Id INTEGER
AS
BEGIN
  SELECT * 
  FROM Category
  WHERE Id = @Id
END
GO

IF OBJECT_ID ('FindCategory') IS NOT NULL 
  DROP PROCEDURE FindCategory
GO
  
CREATE PROCEDURE FindCategory
  @Name NVARCHAR(100) = NULL,
  @Description NVARCHAR(100) = NULL
AS
BEGIN
  SELECT * 
  FROM Category
  WHERE (Name = @Name OR @Name IS NULL)
    AND (Description = @Description OR @Description IS NULL)
END
GO

IF OBJECT_ID ('DeleteCategory') IS NOT NULL 
  DROP PROCEDURE DeleteCategory
GO
  
CREATE PROCEDURE DeleteCategory
  @Id INTEGER
AS
BEGIN
  DELETE 
  FROM Category
  WHERE Id = @Id
END
GO

IF OBJECT_ID ('UpdateCategory') IS NOT NULL 
  DROP PROCEDURE UpdateCategory
GO
  
CREATE PROCEDURE UpdateCategory
  @Id INTEGER,
  @Name NVARCHAR(100),
  @Description NVARCHAR(100)
AS
BEGIN
  UPDATE Category
  SET Name = @Name,
    Description = @Description
  WHERE Id = @Id
END
";

      RunScript(connection, script);
    }

    protected override Repository<Category, TFilter> CreateCategoryRepository<TFilter>(IDbConnection connection)
    {
      return new StoredProcedureRepository<Category, TFilter>(
          connection,
          (c) => c.Id);
    }
  }
}
