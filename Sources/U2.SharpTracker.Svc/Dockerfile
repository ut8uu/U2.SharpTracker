#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["U2.SharpTracker.Svc/U2.SharpTracker.Svc.csproj", "U2.SharpTracker.Svc/"]
RUN dotnet restore "U2.SharpTracker.Svc/U2.SharpTracker.Svc.csproj"
COPY . .
WORKDIR "/src/U2.SharpTracker.Svc"
RUN dotnet build "U2.SharpTracker.Svc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "U2.SharpTracker.Svc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "U2.SharpTracker.Svc.dll"]