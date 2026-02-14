# IdentityService

Microsserviço de identidade e autenticação da plataforma AgroSolutions. Permite login do produtor rural com e-mail e senha e emissão de JWT para acesso aos demais microsserviços.

## Tecnologias

- .NET 8
- PostgreSQL
- JWT
- Docker e Kubernetes (Kind)
- Prometheus e Grafana

## Estrutura

```
src/
├── IdentityService.API
├── IdentityService.Application
├── IdentityService.Domain
└── IdentityService.Infrastructure
tests/
└── IdentityService.Tests
k8s/
├── kind/
└── base/
```

## Pré-requisitos

- .NET 8 SDK
- PostgreSQL

## Configuração

`appsettings.json` / variáveis de ambiente:

- `ConnectionStrings:DefaultConnection` – string de conexão PostgreSQL
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:ExpirationTimeHour`
- `RabbitMq` – opcional, para integração com outros serviços

## Executar

```bash
dotnet restore IdentityService.sln
dotnet run --project src/IdentityService.API
```

Swagger: `http://localhost:5000/swagger` (ou porta configurada).

### Banco com Docker

```bash
docker run --name postgres-identity -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=identity_db -p 5432:5432 -d postgres:15-alpine
```

## Endpoints principais

- `POST /api/auth/login` – Login com e-mail e senha; retorna token JWT
- `POST /api/users` – Cria usuário (requer autorização)
- `POST /api/users/admin` – Cria usuário admin
- `GET /api/users/{id}` – Obtém usuário por id

### Autenticação

Incluir no header: `Authorization: Bearer <token>`.

## Observabilidade

- Prometheus e Grafana configurados em `k8s/`
- Métricas e health check conforme configuração da API

## Testes e CI/CD

```bash
dotnet test tests/IdentityService.Tests.csproj --configuration Release
```

Pipeline GitHub Actions: CI (build + testes) e CD (build e push da imagem Docker para Docker Hub).
