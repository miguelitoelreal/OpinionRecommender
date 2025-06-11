# Utiliza la imagen oficial de .NET para build y runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["OpinionRecommender/OpinionRecommender.csproj", "OpinionRecommender/"]
RUN dotnet restore "OpinionRecommender/OpinionRecommender.csproj"
COPY . .
WORKDIR "/src/OpinionRecommender"
RUN dotnet publish "OpinionRecommender.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpinionRecommender.dll"]
