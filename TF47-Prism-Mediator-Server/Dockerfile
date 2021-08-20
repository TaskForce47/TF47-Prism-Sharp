FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TF47-Prism-Mediator-Server/TF47-Prism-Mediator-Server.csproj", "TF47-Prism-Mediator-Server/"]
RUN dotnet restore "TF47-Prism-Mediator-Server/TF47-Prism-Mediator-Server.csproj"
COPY . .
WORKDIR "/src/TF47-Prism-Mediator-Server"
RUN dotnet build "TF47-Prism-Mediator-Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TF47-Prism-Mediator-Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TF47-Prism-Mediator-Server.dll"]
