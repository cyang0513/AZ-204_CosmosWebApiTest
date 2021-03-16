# Copy Dockerfile to upper folder to run with docker CLI
# docker build . -t <name:tag>

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base #For runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build #For build
WORKDIR /src
COPY ["CosmosWebApiTest/CosmosWebApiTest.csproj", "CosmosWebApiTest/"]
RUN dotnet restore "CosmosWebApiTest/CosmosWebApiTest.csproj"
COPY . .
WORKDIR "/src/CosmosWebApiTest"
RUN dotnet build "CosmosWebApiTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CosmosWebApiTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CosmosWebApiTest.dll"]