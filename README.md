# microservice-fiap-process-video

## Pré-Requisitos
Este projeto foi feito com:
- .NET 8
- Visual Studio 2022
- VS Code
- Docker

## Variáveis de Ambiente
Criar um .env dentro da pasta .\FiapProcessaVideo.WebApi e preencher as variáveis de ambiente a seguir:

```
PORT
S3_BUCKET_NAME
AWS_REGION
AWS_ACCESS_KEY_ID
AWS_SECRET_ACCESS_KEY
AWS_ACCESS_TOKEN
AMQP_URI
AMQP_EXCHANGE
AMQP_QUEUE
```

## Project setup
Na pasta do projeto .\FiapProcessaVideo.WebApi executar o seguinte comando para restaurar todas as dependências de pacote necessários para rodar o projeto:

```
dotnet restore
```

## Compile and run the project

```bash
#  compile:
dotnet build

#  run:
dotnet run
```
