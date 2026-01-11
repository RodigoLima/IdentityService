# Monitoramento com Prometheus e Grafana no AWS EKS

Este guia explica como acessar e usar o monitoramento do projeto no AWS EKS.

## 🚀 Deploy Automático

O Prometheus e Grafana são deployados automaticamente pelo GitHub Actions junto com a aplicação.

## 📊 Acessar os Serviços

### 1. Obter URLs dos LoadBalancers

```bash
# Listar todos os serviços
kubectl get svc -n identityservice

# URL da API
kubectl get svc identityservice-service -n identityservice

# URL do Prometheus
kubectl get svc prometheus -n identityservice

# URL do Grafana
kubectl get svc grafana -n identityservice
```

### 2. Prometheus

Acesse via LoadBalancer:
```bash
PROMETHEUS_URL=$(kubectl get svc prometheus -n identityservice -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')
echo "Prometheus: http://$PROMETHEUS_URL:9090"
```

**Funcionalidades:**
- `/targets` - Ver status dos targets (pods da API)
- `/graph` - Executar queries PromQL
- `/metrics` - Métricas do próprio Prometheus

**Queries úteis:**
```promql
# Taxa de requisições HTTP
rate(http_requests_received_total{namespace="identityservice"}[5m])

# Latência p95
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket{namespace="identityservice"}[5m]))

# Uso de memória
process_working_set_bytes{namespace="identityservice"} / 1024 / 1024

# CPU usage
rate(process_cpu_seconds_total{namespace="identityservice"}[5m]) * 100

# Requisições por status code
sum by (code) (rate(http_requests_received_total{namespace="identityservice"}[5m]))
```

### 3. Grafana

Acesse via LoadBalancer:
```bash
GRAFANA_URL=$(kubectl get svc grafana -n identityservice -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')
echo "Grafana: http://$GRAFANA_URL:3000"
```

**Credenciais padrão:**
- **Usuário:** `admin`
- **Senha:** `admin`

**Dashboard Pré-configurado:**
O dashboard "Identity Service API - AWS EKS" já está provisionado automaticamente com:
- Taxa de requisições HTTP
- Latência (p95)
- Status codes
- Número de pods ativos
- Uso de memória por pod
- CPU usage por pod

## 🔧 Configuração Manual (Opcional)

Se precisar deployar manualmente:

```bash
# 1. Configurar AWS e kubectl
aws eks update-kubeconfig --region us-east-1 --name identityservice-cluster

# 2. Deploy Prometheus
kubectl apply -f k8s/prometheus-config.yaml
kubectl apply -f k8s/prometheus-deployment.yaml

# 3. Deploy Grafana
kubectl apply -f k8s/grafana-deployment.yaml

# 4. Verificar status
kubectl get pods -n identityservice -l app=prometheus
kubectl get pods -n identityservice -l app=grafana

# 5. Aguardar LoadBalancers
kubectl get svc -n identityservice -w
```

## 📈 Verificar Métricas

### Via kubectl port-forward (sem LoadBalancer)

```bash
# Prometheus
kubectl port-forward -n identityservice svc/prometheus 9090:9090
# Acesse: http://localhost:9090

# Grafana
kubectl port-forward -n identityservice svc/grafana 3000:3000
# Acesse: http://localhost:3000
```

### Via API diretamente

```bash
# Métricas da API
API_URL=$(kubectl get svc identityservice-service -n identityservice -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')
curl http://$API_URL/metrics
```

## 🐛 Troubleshooting

### Prometheus não coleta métricas

```bash
# Verificar logs do Prometheus
kubectl logs -n identityservice -l app=prometheus --tail=50

# Verificar targets
kubectl port-forward -n identityservice svc/prometheus 9090:9090
# Abra http://localhost:9090/targets

# Verificar ServiceAccount e RBAC
kubectl get sa prometheus -n identityservice
kubectl get clusterrole prometheus
kubectl get clusterrolebinding prometheus
```

### Grafana não conecta ao Prometheus

```bash
# Verificar logs do Grafana
kubectl logs -n identityservice -l app=grafana --tail=50

# Verificar datasource
kubectl exec -it -n identityservice deployment/grafana -- cat /etc/grafana/provisioning/datasources/datasources.yaml

# Testar conectividade
kubectl exec -it -n identityservice deployment/grafana -- wget -O- http://prometheus:9090/-/healthy
```

### LoadBalancer não provisiona

```bash
# Verificar eventos
kubectl get events -n identityservice --sort-by='.lastTimestamp' | grep LoadBalancer

# Verificar service
kubectl describe svc prometheus -n identityservice
kubectl describe svc grafana -n identityservice

# AWS Load Balancers
aws elbv2 describe-load-balancers --region us-east-1
```

## 🗑️ Remover Monitoramento

```bash
# Remover Grafana
kubectl delete -f k8s/grafana-deployment.yaml

# Remover Prometheus
kubectl delete -f k8s/prometheus-deployment.yaml
kubectl delete -f k8s/prometheus-config.yaml

# Remover RBAC do Prometheus
kubectl delete clusterrolebinding prometheus
kubectl delete clusterrole prometheus
kubectl delete sa prometheus -n identityservice
```

## 📊 Métricas Disponíveis

A aplicação .NET expõe automaticamente métricas via `prometheus-net.AspNetCore`:

### Métricas HTTP
- `http_requests_received_total` - Total de requisições
- `http_request_duration_seconds` - Duração das requisições (histogram)
- `http_requests_in_progress` - Requisições em andamento

### Métricas do .NET
- `process_working_set_bytes` - Memória working set
- `process_cpu_seconds_total` - CPU time total
- `dotnet_collection_count_total` - Garbage collections
- `dotnet_total_memory_bytes` - Memória total

### Métricas do Sistema
- `process_open_handles` - Handles abertos
- `process_num_threads` - Número de threads

## 🔐 Segurança

**⚠️ IMPORTANTE para Produção:**

1. **Alterar senha do Grafana:**
```bash
kubectl set env deployment/grafana -n identityservice GF_SECURITY_ADMIN_PASSWORD=SUA_SENHA_SEGURA
```

2. **Usar Ingress com TLS** em vez de LoadBalancer público

3. **Restringir acesso via Security Groups:**
```bash
# Apenas seu IP
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxx \
  --protocol tcp \
  --port 3000 \
  --cidr SEU_IP/32
```

4. **Usar autenticação OAuth** no Grafana

## 📚 Referências

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [prometheus-net](https://github.com/prometheus-net/prometheus-net)
- [Kubernetes Monitoring](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/)
