FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RatFriendBackend/RatFriendBackend.csproj", "RatFriendBackend/"]
RUN dotnet restore "RatFriendBackend/RatFriendBackend.csproj"
COPY . .
WORKDIR "/src/RatFriendBackend"
RUN dotnet build "RatFriendBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RatFriendBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RatFriendBackend.dll"]
