Sample app to test a few technical details:

- [x] Simple JWT token generator
- [x] Use a permission-based authorization control setup with `PermissionBasedAuthorisation` nuget
- [x] Architecture using `MediatR`
- [x] Docker containers for the web api and the database
- [ ] Logging setup (e.g. `Serilog` + `Kibana`, or similar)
- [x] Integration tests using ASP.NET TestHost and a Docker container for database through `Ductus.FluentDocker`
- [ ] Integration tests using `SpecFlow`

### Pre-requisites
* Docker
* .NET 5 SDK
* Visual Studio 2019

### Getting started
* When opening VS 2019, Docker containers will be created automatically
* Execute `/setup/setup.ps1` to create the database
* Launch the api and navigate to `/migrate` to migrate the database to latest version
* Use Swagger or Postman to test the endpoints
