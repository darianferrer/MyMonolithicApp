#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/MyMonolithicApp.Host/MyMonolithicApp.Host.csproj", "src/MyMonolithicApp.Host/"]
COPY ["src/MyMonolithicApp.Api/MyMonolithicApp.Api.csproj", "src/MyMonolithicApp.Api/"]
COPY ["src/MyMonolithicApp.Application/MyMonolithicApp.Application.csproj", "src/MyMonolithicApp.Application/"]
COPY ["src/MyMonolithicApp.Domain/MyMonolithicApp.Domain.csproj", "src/MyMonolithicApp.Domain/"]
COPY ["src/MyMonolithicApp.Infrastructure/MyMonolithicApp.Infrastructure.csproj", "src/MyMonolithicApp.Infrastructure/"]
RUN dotnet restore "src/MyMonolithicApp.Host/MyMonolithicApp.Host.csproj"
COPY . .
WORKDIR "/src/src/MyMonolithicApp.Host"
RUN dotnet build "MyMonolithicApp.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyMonolithicApp.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyMonolithicApp.Host.dll"]
