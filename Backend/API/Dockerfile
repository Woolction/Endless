FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore API/API.csproj

RUN dotnet publish API/API.csproj -c Release -o /out 

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /out .

ENTRYPOINT ["dotnet", "API.dll"]