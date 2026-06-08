FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/API/API.csproj src/API/
COPY src/Infrastructure/Authentication/Authentication.csproj src/Infrastructure/Authentication/
COPY src/Infrastructure/Recommendation/Recommendation.csproj src/Infrastructure/Recommendation/
COPY src/Infrastructure/Persistence/Persistence.csproj src/Infrastructure/Persistence/
COPY src/Infrastructure/Messaging/Messaging.csproj src/Infrastructure/Messaging/
COPY src/Infrastructure/Storage/Storage.csproj src/Infrastructure/Storage/
COPY src/Infrastructure/Media/Media.csproj src/Infrastructure/Media/
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