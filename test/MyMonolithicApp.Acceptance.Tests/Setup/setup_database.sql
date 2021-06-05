USE master
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'MyMonolithicApp')
BEGIN
  CREATE DATABASE [MyMonolithicApp];
END;
GO