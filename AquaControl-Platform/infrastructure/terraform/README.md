# AquaControl Platform - AWS Infrastructure as Code

This directory contains Terraform configurations for deploying the AquaControl Platform on AWS.

## Architecture Overview

The infrastructure includes:

- **EKS Cluster** - Kubernetes cluster for containerized applications
- **RDS PostgreSQL** - Managed database with TimescaleDB extension
- **ElastiCache Redis** - Managed Redis cluster for caching
- **S3 Buckets** - Application data and backup storage
- **EFS** - Shared file storage for Kubernetes
- **VPC** - Isolated network with public/private subnets
- **CloudWatch** - Monitoring, logging, and alerting
- **Prometheus & Grafana** - Advanced monitoring stack
- **X-Ray** - Distributed tracing

## Prerequisites

1. **AWS Account** with appropriate permissions
2. **Terraform** >= 1.0 installed
3. **AWS CLI** configured with credentials
4. **kubectl** and **helm** for Kubernetes management
5. **S3 Bucket** for Terraform state (create manually first):
   ```bash
   aws s3 mb s3://aquacontrol-terraform-state --region us-east-1
   aws dynamodb create-table \
     --table-name aquacontrol-terraform-locks \
     --attribute-definitions AttributeName=LockID,AttributeType=S \
     --key-schema AttributeName=LockID,KeyType=HASH \
     --billing-mode PAY_PER_REQUEST \
     --region us-east-1
   ```

## Quick Start

1. **Copy example variables file:**
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

2. **Edit terraform.tfvars** with your configuration:
   ```hcl
   environment = "dev"
   aws_region  = "us-east-1"
   alert_email_addresses = ["admin@example.com"]
   ```

3. **Initialize Terraform:**
   ```bash
   terraform init
   ```

4. **Plan the deployment:**
   ```bash
   terraform plan
   ```

5. **Apply the infrastructure:**
   ```bash
   terraform apply
   ```

6. **Configure kubectl:**
   ```bash
   aws eks update-kubeconfig --name aquacontrol-dev-eks --region us-east-1
   ```

## File Structure

- `main.tf` - Main Terraform configuration and providers
- `vpc.tf` - VPC, subnets, security groups, and VPC endpoints
- `eks.tf` - EKS cluster and node groups configuration
- `databases.tf` - RDS PostgreSQL and ElastiCache Redis
- `storage.tf` - S3 buckets, KMS keys, and EFS
- `monitoring.tf` - CloudWatch, alarms, dashboards, and X-Ray
- `k8s-addons.tf` - Kubernetes addons (Load Balancer Controller, Autoscaler, Prometheus, Grafana)
- `variables.tf` - Input variables
- `outputs.tf` - Output values

## Environments

The infrastructure supports multiple environments:

- **dev** - Development environment (smaller instances, single NAT gateway)
- **staging** - Staging environment (medium instances)
- **prod** - Production environment (larger instances, read replicas, deletion protection)

Set the environment using the `environment` variable in `terraform.tfvars`.

## Key Features

### High Availability
- Multi-AZ deployment for RDS and ElastiCache
- EKS node groups across multiple availability zones
- Read replicas in production

### Security
- VPC with private subnets for databases
- Security groups with least privilege access
- Encryption at rest for all storage
- KMS encryption for sensitive data
- IAM roles with minimal permissions

### Monitoring
- CloudWatch metrics and alarms
- Prometheus and Grafana dashboards
- X-Ray distributed tracing
- SNS alerts for critical issues

### Cost Optimization
- Right-sized instances per environment
- S3 lifecycle policies for archival
- Single NAT gateway in dev environment
- VPC endpoints to reduce data transfer costs

## Outputs

After deployment, Terraform outputs include:

- EKS cluster endpoint and name
- RDS endpoint (sensitive)
- Redis endpoint (sensitive)
- S3 bucket names
- Grafana admin password (sensitive)

Access outputs with:
```bash
terraform output
terraform output -json
```

## Destroying Infrastructure

⚠️ **Warning**: This will delete all resources!

```bash
terraform destroy
```

Note: Production environment has deletion protection enabled. Disable it first if needed.

## Troubleshooting

### EKS Cluster Access Issues
```bash
# Update kubeconfig
aws eks update-kubeconfig --name <cluster-name> --region <region>

# Verify access
kubectl get nodes
```

### Terraform State Issues
```bash
# Refresh state
terraform refresh

# Import existing resources (if needed)
terraform import <resource_type>.<name> <resource_id>
```

### Database Connection Issues
- Verify security groups allow traffic from EKS nodes
- Check RDS endpoint in outputs
- Ensure credentials are correct

## Next Steps

After infrastructure is deployed:

1. Deploy application to EKS
2. Configure CI/CD pipelines
3. Set up application monitoring
4. Configure backup schedules
5. Review and tune security groups

## Support

For issues or questions, refer to:
- [Terraform AWS Provider Documentation](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [EKS Documentation](https://docs.aws.amazon.com/eks/)
- [Phase 1 AWS DevOps Documentation](../../../Phase1-Advanced-Implementation/06-ADVANCED_PHASE1_AWS_DEVOPS.md)

