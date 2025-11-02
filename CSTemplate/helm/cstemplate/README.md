# CSTemplate Helm Chart

This Helm chart deploys the CSTemplate Weather API to Kubernetes.

## Prerequisites

- Kubernetes 1.19+
- Helm 3.x
- PostgreSQL database (included or external)

## Installation

### Quick Start

```bash
# Create namespace
kubectl create namespace cstemplate

# Create database secret
kubectl create secret generic postgres-secret \
  --from-literal=password=your-secure-password \
  --namespace cstemplate

# Install chart
helm install cstemplate ./helm/cstemplate \
  --namespace cstemplate
```

### Staging Environment

```bash
# Create namespace
kubectl create namespace cstemplate-staging

# Create database secret
kubectl create secret generic postgres-staging-secret \
  --from-literal=password=your-secure-password \
  --namespace cstemplate-staging

# Install with staging values
helm install cstemplate-staging ./helm/cstemplate \
  --values ./helm/cstemplate/values-staging.yaml \
  --namespace cstemplate-staging
```

## Configuration

### Common Values

| Parameter | Description | Default |
|-----------|-------------|---------|
| `replicaCount` | Number of replicas | `1` |
| `image.repository` | Image repository | `cstemplate-api` |
| `image.tag` | Image tag | `latest` |
| `service.type` | Service type | `ClusterIP` |
| `service.port` | Service port | `80` |
| `ingress.enabled` | Enable ingress | `false` |
| `autoscaling.enabled` | Enable HPA | `false` |

### Database Configuration

| Parameter | Description | Default |
|-----------|-------------|---------|
| `database.host` | Database host | `postgres-service` |
| `database.port` | Database port | `5432` |
| `database.name` | Database name | `weatherdb` |
| `database.username` | Database username | `postgres` |
| `database.passwordSecret.name` | Secret name for password | `postgres-secret` |
| `database.passwordSecret.key` | Secret key for password | `password` |
| `postgresql.enabled` | Deploy PostgreSQL subchart | `true` |

### Resources

| Parameter | Description | Default |
|-----------|-------------|---------|
| `resources.limits.cpu` | CPU limit | `500m` |
| `resources.limits.memory` | Memory limit | `512Mi` |
| `resources.requests.cpu` | CPU request | `250m` |
| `resources.requests.memory` | Memory request | `256Mi` |

## Staging-Specific Configuration

The `values-staging.yaml` file includes production-ready defaults:

- **Replicas**: 2 (with HPA enabled, scales 2-5)
- **Ingress**: Enabled with TLS
- **Autoscaling**: Enabled (CPU: 70%, Memory: 80%)
- **Resources**: 500m/512Mi requests, 1000m/1Gi limits
- **Database**: Dedicated staging database with 20Gi storage
- **Node Affinity**: Prefers staging node pool

## Upgrading

```bash
# Upgrade with default values
helm upgrade cstemplate ./helm/cstemplate \
  --namespace cstemplate

# Upgrade staging
helm upgrade cstemplate-staging ./helm/cstemplate \
  --values ./helm/cstemplate/values-staging.yaml \
  --namespace cstemplate-staging
```

## Uninstalling

```bash
helm uninstall cstemplate --namespace cstemplate
```

## Customization

Create your own values file:

```yaml
# my-values.yaml
replicaCount: 3

image:
  repository: myregistry.azurecr.io/cstemplate-api
  tag: v1.0.0

ingress:
  enabled: true
  hosts:
    - host: api.mydomain.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: api-tls
      hosts:
        - api.mydomain.com

database:
  host: external-postgres.database.azure.com
  passwordSecret:
    name: external-db-secret
    key: password

postgresql:
  enabled: false  # Using external database
```

Install with custom values:

```bash
helm install cstemplate ./helm/cstemplate \
  --values my-values.yaml \
  --namespace cstemplate
```

## Monitoring

The chart includes:
- Liveness probe on `/weatherforecast`
- Readiness probe on `/weatherforecast`
- Prometheus annotations (in staging)

## Troubleshooting

### Check pod status
```bash
kubectl get pods -n cstemplate
kubectl logs -f deployment/cstemplate -n cstemplate
```

### Check database connectivity
```bash
kubectl exec -it deployment/cstemplate -n cstemplate -- \
  env | grep ConnectionStrings
```

### Verify secret
```bash
kubectl get secret postgres-secret -n cstemplate -o yaml
```

### Test service
```bash
kubectl port-forward svc/cstemplate 8080:80 -n cstemplate
curl http://localhost:8080/weatherforecast
```

