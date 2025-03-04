FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#USER $APP_UID
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081 

#VOLUME /app/data

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Blog.Api/Blog.Api.csproj", "Blog.Api/"]
RUN dotnet restore "./Blog.Api/Blog.Api.csproj"
COPY . .
WORKDIR "/src/Blog.Api"
RUN dotnet build "./Blog.Api.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Blog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blog.Api.dll"]
