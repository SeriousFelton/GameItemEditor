FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GameItemEditor.Api/GameItemEditor.Api.csproj", "GameItemEditor.Api/"]
COPY ["GameItemEditor.Core/GameItemEditor.Core.csproj", "GameItemEditor.Core/"]
RUN dotnet restore "GameItemEditor.Api/GameItemEditor.Api.csproj"
COPY . .
WORKDIR "/src/GameItemEditor.Api"
RUN dotnet build "GameItemEditor.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameItemEditor.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameItemEditor.Api.dll"]