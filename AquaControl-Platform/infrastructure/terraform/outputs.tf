output "vpc_id" {
  description = "ID of the VPC"
  value       = module.vpc.vpc_id
}

output "eks_cluster_endpoint" {
  description = "Endpoint for EKS control plane"
  value       = module.eks.cluster_endpoint
}

output "eks_cluster_name" {
  description = "Name of the EKS cluster"
  value       = module.eks.cluster_name
}

output "rds_endpoint" {
  description = "RDS instance endpoint"
  value       = aws_db_instance.main.endpoint
  sensitive   = true
}

output "rds_password" {
  description = "RDS instance password"
  value       = random_password.db_password.result
  sensitive   = true
}

output "redis_endpoint" {
  description = "ElastiCache Redis endpoint"
  value       = aws_elasticache_replication_group.main.primary_endpoint_address
  sensitive   = true
}

output "redis_password" {
  description = "ElastiCache Redis password"
  value       = random_password.redis_password.result
  sensitive   = true
}

output "s3_app_data_bucket" {
  description = "S3 bucket for application data"
  value       = aws_s3_bucket.app_data.id
}

output "s3_backups_bucket" {
  description = "S3 bucket for backups"
  value       = aws_s3_bucket.backups.id
}

output "grafana_admin_password" {
  description = "Grafana admin password"
  value       = random_password.grafana_admin.result
  sensitive   = true
}

output "cloudwatch_log_group" {
  description = "CloudWatch log group for applications"
  value       = aws_cloudwatch_log_group.application.name
}

