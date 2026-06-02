FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore 

FROM build AS test-runner
RUN dotnet test tests/tests.csproj -c Release --no-restore

FROM build AS publish
RUN dotnet publish src/API/API.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0

# ffmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=publish /out .

ENTRYPOINT ["dotnet", "API.dll"]