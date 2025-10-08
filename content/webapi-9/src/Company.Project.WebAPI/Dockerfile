# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/GreatIdeas.WebAPI/GreatIdeas.WebAPI.csproj", "src/GreatIdeas.WebAPI/"]
COPY ["aspire/GreatIdeas.ServiceDefaults/GreatIdeas.ServiceDefaults.csproj", "aspire/GreatIdeas.ServiceDefaults/"]
COPY ["src/GreatIdeas.Application/Company.Project.Application.csproj", "src/GreatIdeas.Application/"]
COPY ["src/GreatIdeas.Domain/GreatIdeas.Domain.csproj", "src/GreatIdeas.Domain/"]
COPY ["src/GreatIdeas.Infrastructure/GreatIdeas.Infrastructure.csproj", "src/GreatIdeas.Infrastructure/"]
RUN dotnet restore "./src/GreatIdeas.WebAPI/GreatIdeas.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/GreatIdeas.WebAPI"
RUN dotnet build "./GreatIdeas.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GreatIdeas.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreatIdeas.WebAPI.dll"]