# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BankAccountSolution.sln .
COPY BankAccountAPI/BankAccountAPI.csproj BankAccountAPI/
COPY BankAccountAPI.Tests/BankAccountAPI.Tests.csproj BankAccountAPI.Tests/

# Restore dependencies
RUN dotnet restore BankAccountAPI/BankAccountAPI.csproj

# Copy source code
COPY BankAccountAPI/ BankAccountAPI/

# Build the application
WORKDIR /src/BankAccountAPI
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "BankAccountAPI.dll"]
