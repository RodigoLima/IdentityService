#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
KIND_NAME="dev-identityservice"
NAMESPACE="identityservice"
ENV_FILE="$ROOT_DIR/.env"

DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-docker}"
DB_PASSWORD="${DB_PASSWORD:-docker}"
DB_NAME="${DB_NAME:-IdentityService}"
RABBITMQ_HOST="${RABBITMQ_HOST:-localhost}"
RABBITMQ_PORT="${RABBITMQ_PORT:-5672}"
RABBITMQ_USER="${RABBITMQ_USER:-guest}"
RABBITMQ_PASSWORD="${RABBITMQ_PASSWORD:-guest}"
RABBITMQ_VHOST="${RABBITMQ_VHOST:-/}"
JWT_KEY="${JWT_KEY:-7G+H65bLToXxqzPvj7+q0oQUlxJp1WvdOU3nv3ArA1s=}"
ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"

[ -f "$ENV_FILE" ] && set -a && source "$ENV_FILE" && set +a

echo "================================================"
echo "  IdentityService - Build & Deploy (Kind)"
echo "================================================"

command -v kind >/dev/null 2>&1 || { echo "Kind não instalado."; exit 1; }
command -v kubectl >/dev/null 2>&1 || { echo "kubectl não instalado."; exit 1; }
command -v docker >/dev/null 2>&1 || { echo "Docker não instalado."; exit 1; }

if ! kind get clusters 2>/dev/null | grep -q "^${KIND_NAME}$"; then
  echo "Criando cluster Kind ${KIND_NAME}..."
  kind create cluster --config "$ROOT_DIR/k8s/kind/config.yaml"
fi

echo "Build e carregamento da imagem..."
docker build -t identityservice-api:dev "$ROOT_DIR"
kind load docker-image identityservice-api:dev --name "$KIND_NAME"

ctx=$(kubectl config current-context 2>/dev/null)
[[ "$ctx" != *"$KIND_NAME"* ]] && { echo "Contexto Kind não está ativo."; exit 1; }

echo "Aplicando namespace..."
kubectl apply -f "$ROOT_DIR/k8s/base/namespaces/identityservice.yaml"

echo "Criando secret database-config..."
kubectl create secret generic database-config -n "$NAMESPACE" \
  --from-literal=DB_HOST="$DB_HOST" \
  --from-literal=DB_PORT="$DB_PORT" \
  --from-literal=DB_USER="$DB_USER" \
  --from-literal=DB_PASSWORD="$DB_PASSWORD" \
  --from-literal=DB_NAME="$DB_NAME" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Aplicando Postgres (PV, PVC, Deployment, Service)..."
kubectl apply -f "$ROOT_DIR/k8s/base/postgresql/pv.yaml"
kubectl apply -f "$ROOT_DIR/k8s/base/postgresql/pvc.yaml"
kubectl apply -f "$ROOT_DIR/k8s/base/postgresql/deployment.yaml"
kubectl apply -f "$ROOT_DIR/k8s/base/postgresql/service.yaml"

echo "Aguardando Postgres ficar pronto..."
kubectl wait --for=condition=ready pod -l app=postgres -n "$NAMESPACE" --timeout=120s 2>/dev/null || sleep 15

CONN="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
echo "Criando secret identityservice-secret..."
kubectl create secret generic identityservice-secret -n "$NAMESPACE" \
  --from-literal=ConnectionStrings__DefaultConnection="$CONN" \
  --from-literal=ConnectionStrings__PostgreSql="Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};Port=${DB_PORT}" \
  --from-literal=Jwt__Key="$JWT_KEY" \
  --from-literal=RabbitMq__Host="$RABBITMQ_HOST" \
  --from-literal=RabbitMq__Port="$RABBITMQ_PORT" \
  --from-literal=RabbitMq__Username="$RABBITMQ_USER" \
  --from-literal=RabbitMq__Password="$RABBITMQ_PASSWORD" \
  --from-literal=RabbitMq__VirtualHost="$RABBITMQ_VHOST" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Criando ConfigMap..."
kubectl create configmap identityservice-config -n "$NAMESPACE" \
  --from-literal=ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENVIRONMENT" \
  --from-literal=ASPNETCORE_URLS="http://+:8080" \
  --from-literal=DatabaseProvider="PostgreSql" \
  --from-literal=Jwt__ExpirationTimeHour="5" \
  --from-literal=Jwt__IncreaseExpirationTimeMinutes="20" \
  --from-literal=PATH_BASE="/api/identity" \
  --from-literal=RabbitMq__Host="$RABBITMQ_HOST" \
  --from-literal=RabbitMq__Port="$RABBITMQ_PORT" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Aplicando API..."
kubectl apply -f "$ROOT_DIR/k8s/base/app/deployment.yaml"
kubectl apply -f "$ROOT_DIR/k8s/base/app/service.yaml"

echo "Aguardando API..."
kubectl wait --for=condition=ready pod -l app=identityservice-api -n "$NAMESPACE" --timeout=120s 2>/dev/null || true

echo ""
echo "Deploy concluído. Pods:"
kubectl get pods -n "$NAMESPACE"
echo ""
echo "API (NodePort 30080): http://localhost:30080"
