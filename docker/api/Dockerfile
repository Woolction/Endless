FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/API/API.csproj src/API/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Application/Application.csproj src/Application/
COPY src/Domain/Domain.csproj src/Domain/

COPY tests/tests.csproj tests/

RUN dotnet restore src/API/API.csproj
RUN dotnet restore tests/tests.csproj

COPY . .

RUN dotnet publish src/API/API.csproj -c Release -o /app/publish --no-restore

FROM build AS test-runner
RUN dotnet test tests/tests.csproj -c Release --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]