# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Instala o ffmpeg e copia o ffprobe para o diretório /app
RUN apt-get update && apt-get install -y ffmpeg && \
    mkdir -p /app && \
    cp /usr/bin/ffprobe /app/ffprobe && \
    chown -R app:app /app && \
    rm -rf /var/lib/apt/lists/*

# Volta para o usuário "app" para maior segurança
USER app

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FiapProcessaVideo.WebApi/FiapProcessaVideo.WebApi.csproj", "FiapProcessaVideo.WebApi/"]
COPY ["FiapProcessaVideo.Domain/FiapProcessaVideo.Domain.csproj", "FiapProcessaVideo.Domain/"]
COPY ["FiapProcessaVideo.Application/FiapProcessaVideo.Application.csproj", "FiapProcessaVideo.Application/"]
RUN dotnet restore "./FiapProcessaVideo.WebApi/FiapProcessaVideo.WebApi.csproj"
COPY . .
WORKDIR "/src/FiapProcessaVideo.WebApi"
RUN dotnet build "FiapProcessaVideo.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FiapProcessaVideo.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FiapProcessaVideo.WebApi.dll"]
