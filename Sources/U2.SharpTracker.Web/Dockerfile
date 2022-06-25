#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["U2.SharpTracker.Web/U2.SharpTracker.Web.csproj", "U2.SharpTracker.Web/"]
RUN dotnet restore "U2.SharpTracker.Web/U2.SharpTracker.Web.csproj"
COPY . .
WORKDIR "/src/U2.SharpTracker.Web"
RUN dotnet build "U2.SharpTracker.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "U2.SharpTracker.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "U2.SharpTracker.Web.dll"]