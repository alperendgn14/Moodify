# 1-derleme
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# tüm dosyaları kopyala
COPY . ./

# bağımlılıkları yükle ve projeyi derle 
RUN dotnet restore
RUN dotnet publish -c Release -o out

# 2-çalıştırma
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

# render'ın varsayılan olarak dinlediği 8080 portunu aç
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080


ENTRYPOINT ["dotnet", "SpotifyAI.dll"]