# IdentityService

Microserviço de autenticação e gerenciamento de identidade desenvolvido com .NET 8, seguindo os princípios de Domain-Driven Design (DDD).

## 🚀 Tecnologias

- **.NET 8** com C# 12
- **PostgreSQL** (AWS RDS)
- **Docker** & **Kubernetes** (Amazon EKS)
- **JWT** para autenticação
- **Prometheus** & **Grafana** para monitoramento
- **AWS Cloud** (EKS, RDS, ECR, VPC, LoadBalancer)

## 📁 Estrutura do Projeto

A arquitetura segue o padrão **DDD (Domain-Driven Design)**:

```
src/
├── IdentityService.API          → Camada de apresentação (API REST)
├── IdentityService.Application   → Casos de uso e lógica de aplicação
├── IdentityService.Domain        → Entidades de domínio e contratos
└── IdentityService.Infrastructure → Implementações de persistência e serviços externos
```

## 🏗️ Arquitetura AWS

```
┌─────────────────────────────────────────────────┐
│                  AWS Cloud                       │
│  ┌───────────────────────────────────────────┐  │
│  │            VPC (us-east-1)                │  │
│  │  ┌─────────────────┐  ┌────────────────┐ │  │
│  │  │   EKS Cluster   │  │   RDS Instance │ │  │
│  │  │ identityservice │  │   PostgreSQL   │ │  │
│  │  │                 │  │   identity_db  │ │  │
│  │  │  ┌───────────┐  │  └────────────────┘ │  │
│  │  │  │  Pod 1    │  │         ↑           │  │
│  │  │  │identity-api│  │         │           │  │
│  │  │  └───────────┘  │         │           │  │
│  │  │  ┌───────────┐  │    Connection       │  │
│  │  │  │  Pod 2    │──┼─────────┘           │  │
│  │  │  │identity-api│  │                     │  │
│  │  │  └───────────┘  │                     │  │
│  │  └─────────────────┘                     │  │
│  └───────────────────────────────────────────┘  │
│           ↑                                      │
│    LoadBalancer (NLB)                            │
└───────────┼────────────────────────────────────┘
            │
        Internet
```

## 🛠️ Como Executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [PostgreSQL](https://www.postgresql.org/download/) ou Docker

### Desenvolvimento Local

#### 1. Banco de Dados com Docker

```bash
docker run --name postgres-local \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=identity_db \
  -p 5432:5432 \
  -d postgres:15-alpine
```

#### 2. Configurar appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Database=identity_db;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long-base64-encoded"
  }
}
```

#### 3. Executar Migrations

```bash
dotnet ef migrations add InitialCreate \
  --project src/IdentityService.Infrastructure \
  --startup-project src/IdentityService.API

dotnet ef database update \
  --project src/IdentityService.Infrastructure \
  --startup-project src/IdentityService.API
```

#### 4. Executar a Aplicação

```bash
cd src/IdentityService.API
dotnet run
```

Acesse: `http://localhost:5000/swagger`

### Docker Compose

```bash
docker-compose up -d
```

Isso iniciará:
- PostgreSQL na porta 5433
- API na porta 8080
- Prometheus na porta 9090
- Grafana na porta 3000

## 📝 Endpoints Principais

### Autenticação

- `POST /accounts/token` - Gera token JWT (público)
- `POST /users` - Cria novo usuário (requer Admin)
- `POST /users/admin` - Cria usuário admin (público)

### Monitoramento

- `GET /health` - Health check
- `GET /metrics` - Métricas Prometheus
- `GET /swagger` - Documentação Swagger

## 🔐 Autenticação

O serviço utiliza JWT (JSON Web Tokens) para autenticação. Para usar os endpoints protegidos:

1. Obtenha um token via `POST /accounts/token`
2. Inclua o token no header: `Authorization: Bearer <token>`

### Níveis de Acesso

- **Admin**: Acesso completo
- **User**: Acesso limitado
- **Guest**: Acesso básico

## 🚢 Deploy na AWS

### Pré-requisitos

- [AWS CLI](https://aws.amazon.com/cli/)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- Conta AWS configurada

### 1. Criar EKS Cluster

```bash
aws eks create-cluster \
  --name identityservice-cluster \
  --role-arn arn:aws:iam::ACCOUNT_ID:role/LabRole \
  --resources-vpc-config subnetIds=subnet-xxx,subnet-yyy,securityGroupIds=sg-xxx \
  --region us-east-1

aws eks wait cluster-active --name identityservice-cluster --region us-east-1
aws eks update-kubeconfig --region us-east-1 --name identityservice-cluster
```

### 2. Criar RDS PostgreSQL

```bash
aws rds create-db-instance \
  --db-instance-identifier identityservice-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --engine-version 15.8 \
  --master-username postgres \
  --master-user-password YOUR_PASSWORD \
  --allocated-storage 20 \
  --vpc-security-group-ids sg-xxx \
  --region us-east-1
```

### 3. Criar ECR Repository

```bash
aws ecr create-repository \
  --repository-name identityservice-api \
  --region us-east-1
```

### 4. Configurar Secrets no Kubernetes

```bash
kubectl create namespace identityservice

kubectl create secret generic identityservice-secret \
  --from-literal=ConnectionStrings__PostgreSql="Host=YOUR_RDS_ENDPOINT;Database=identity_db;Username=postgres;Password=YOUR_PASSWORD" \
  --from-literal=Jwt__Key="YOUR_JWT_KEY_MIN_32_CHARS_BASE64" \
  --namespace=identityservice
```

### 5. Deploy

```bash
# Aplicar manifests
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

## 📊 Monitoramento

### Prometheus

Acesse: `http://<PROMETHEUS_URL>:9090`

### Grafana

Acesse: `http://<GRAFANA_URL>:3000`
- Usuário: `admin`
- Senha: `admin`

Dashboard pré-configurado: "Identity Service API - AWS EKS"

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 Documentação Adicional

- [Arquitetura AWS](./docs/architecture.md)
- [Guia de Deploy](./docs/deployment.md)
- [Monitoramento](./k8s/MONITORING.md)

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor, abra uma issue ou envie um pull request.

## 📄 Licença

Este projeto está licenciado sob a **MIT License**.
