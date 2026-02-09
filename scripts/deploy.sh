#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
KIND_NAME="agro-dev"
NAMESPACE="identityservice"
ENV_FILE="$ROOT_DIR/.env"

DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-docker}"
DB_PASSWORD="${DB_PASSWORD:-docker}"
DB_NAME="${DB_NAME:-IdentityService}"
RABBITMQ_HOST="${RABBITMQ_HOST:-rabbitmq-service.sensor-ingestion.svc.cluster.local}"
RABBITMQ_PORT="${RABBITMQ_PORT:-5672}"
RABBITMQ_USER="${RABBITMQ_USER:-admin}"
RABBITMQ_PASSWORD="${RABBITMQ_PASSWORD:-admin123}"
RABBITMQ_VHOST="${RABBITMQ_VHOST:-/}"
JWT_KEY="${JWT_KEY:-7G+H65bLToXxqzPvj7+q0oQUlxJp1WvdOU3nv3ArA1s=}"
DEFAULT_ADMIN_EMAIL="${DEFAULT_ADMIN_EMAIL:-admin@localhost}"
DEFAULT_ADMIN_PASSWORD="${DEFAULT_ADMIN_PASSWORD:-Admin@123}"
ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"

[ -f "$ENV_FILE" ] && set -a && source "$ENV_FILE" && set +a
[[ -z "$RABBITMQ_VHOST" || "$RABBITMQ_VHOST" == *":"* || "$RABBITMQ_VHOST" == *"Program"* ]] && RABBITMQ_VHOST="/"

echo "================================================"
echo "  IdentityService - Build & Deploy (Kind)"
echo "================================================"

command -v kind >/dev/null 2>&1 || { echo "Kind não instalado."; exit 1; }
command -v kubectl >/dev/null 2>&1 || { echo "kubectl não instalado."; exit 1; }
command -v docker >/dev/null 2>&1 || { echo "Docker não instalado."; exit 1; }

if ! kind get clusters 2>/dev/null | grep -q "^${KIND_NAME}$"; then
  echo "Criando cluster Kind ${KIND_NAME}..."
  kind create cluster --config "$ROOT_DIR/k8s/kind/config.yaml"
else
  [ "$(kubectl config current-context 2>/dev/null)" != "kind-${KIND_NAME}" ] && kubectl config use-context kind-${KIND_NAME} 2>/dev/null || true
fi

if [ -z "${SKIP_BUILD:-}" ]; then
  echo "Build e carregamento da imagem..."
  docker build -t identityservice-api:dev "$ROOT_DIR"
  kind load docker-image identityservice-api:dev --name "$KIND_NAME"
fi

ctx=$(kubectl config current-context 2>/dev/null)
[[ "$ctx" != *"$KIND_NAME"* ]] && { echo "Contexto Kind não está ativo."; exit 1; }

PROJECTS_ROOT="${PROJECTS_ROOT:-$(cd "$ROOT_DIR/.." && pwd)}"
DATA_INGESTION_ROOT="${DATA_INGESTION_ROOT:-$PROJECTS_ROOT/AgroSolutions.DataIngestion}"
WAIT_TO="${WAIT_TIMEOUT:-45}"
if [ -d "$DATA_INGESTION_ROOT/k8s" ]; then
  if ! kubectl get ns sensor-ingestion &>/dev/null; then
    echo "Aplicando RabbitMQ (dependência)..."
    kubectl apply -f "$DATA_INGESTION_ROOT/k8s/namespaces.yaml"
    kubectl apply -f "$DATA_INGESTION_ROOT/k8s/secrets.yaml"
    kubectl apply -f "$DATA_INGESTION_ROOT/k8s/infra/rabbitmq"
    kubectl wait --for=condition=ready pod -l app=rabbitmq -n sensor-ingestion --timeout="${WAIT_TO}s" 2>/dev/null || sleep 10
  else
    kubectl apply -f "$DATA_INGESTION_ROOT/k8s/secrets.yaml" 2>/dev/null || true
    echo "Aguardando RabbitMQ..."
    kubectl wait --for=condition=ready pod -l app=rabbitmq -n sensor-ingestion --timeout="${WAIT_TO}s" 2>/dev/null || sleep 5
  fi
fi

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

WAIT_TO="${WAIT_TIMEOUT:-45}"
if kubectl wait --for=condition=ready pod -l app=postgres -n "$NAMESPACE" --timeout=0s 2>/dev/null; then echo "Postgres já pronto."; else echo "Aguardando Postgres..."; kubectl wait --for=condition=ready pod -l app=postgres -n "$NAMESPACE" --timeout="${WAIT_TO}s" 2>/dev/null || sleep 5; fi

CONN="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
echo "Criando secret identityservice-secret..."
kubectl create secret generic identityservice-secret -n "$NAMESPACE" \
  --from-literal=DefaultAdmin__Email="$DEFAULT_ADMIN_EMAIL" \
  --from-literal=DefaultAdmin__Password="$DEFAULT_ADMIN_PASSWORD" \
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

echo "Aplicando observabilidade (Prometheus + Grafana)..."
kubectl apply -f "$ROOT_DIR/k8s/prometheus-config.yaml"
kubectl apply -f "$ROOT_DIR/k8s/prometheus-deployment.yaml"
kubectl apply -f "$ROOT_DIR/k8s/grafana-deployment.yaml"

WAIT_TO="${WAIT_TIMEOUT:-45}"
if kubectl wait --for=condition=ready pod -l app=identityservice-api -n "$NAMESPACE" --timeout=0s 2>/dev/null; then echo "API já pronta."; else echo "Aguardando API..."; kubectl wait --for=condition=ready pod -l app=identityservice-api -n "$NAMESPACE" --timeout="${WAIT_TO}s" || { echo "API não ficou pronta. Verifique: kubectl get pods -n $NAMESPACE"; exit 1; }; fi

echo ""
echo "Deploy concluído. Pods:"
kubectl get pods -n "$NAMESPACE"
echo ""
echo "================================================"
echo "  IdentityService - URLs e acesso"
echo "================================================"
echo ""
echo "APIs:"
echo "  Identity:       http://localhost:30081/swagger"
echo ""
echo "Infra:"
echo "  Grafana:        http://localhost:30381 (admin/admin)"
echo "  Prometheus:     http://localhost:30981"
echo ""
