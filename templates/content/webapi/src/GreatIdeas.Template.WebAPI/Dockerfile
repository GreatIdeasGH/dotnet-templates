# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/GreatIdeas.Template.WebAPI/GreatIdeas.Template.WebAPI.csproj", "src/GreatIdeas.Template.WebAPI/"]
COPY ["aspire/GreatIdeas.Template.ServiceDefaults/GreatIdeas.Template.ServiceDefaults.csproj", "aspire/GreatIdeas.Template.ServiceDefaults/"]
COPY ["src/GreatIdeas.Template.Application/GreatIdeas.Template.Application.csproj", "src/GreatIdeas.Template.Application/"]
COPY ["src/GreatIdeas.Template.Domain/GreatIdeas.Template.Domain.csproj", "src/GreatIdeas.Template.Domain/"]
COPY ["src/GreatIdeas.Template.Infrastructure/GreatIdeas.Template.Infrastructure.csproj", "src/GreatIdeas.Template.Infrastructure/"]
RUN dotnet restore "./src/GreatIdeas.Template.WebAPI/GreatIdeas.Template.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/GreatIdeas.Template.WebAPI"
RUN dotnet build "./GreatIdeas.Template.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GreatIdeas.Template.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreatIdeas.Template.WebAPI.dll"]