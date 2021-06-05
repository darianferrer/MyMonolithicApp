$User='sa'
$Pwd='Abcd*1234'
$Database='MyMonolithicApp'
$Server='localhost,1433'

sqlcmd -S $Server -U $User -P $Pwd -b -Q "
  IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'$Database')
  BEGIN
    CREATE DATABASE [$Database]
  END;
  GO
"
