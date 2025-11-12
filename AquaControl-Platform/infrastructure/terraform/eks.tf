module "eks" {
  source = "terraform-aws-modules/eks/aws"
  version = "~> 19.15"

  cluster_name    = "${local.name_prefix}-eks"
  cluster_version = "1.27"

  vpc_id                         = module.vpc.vpc_id
  subnet_ids                     = module.vpc.private_subnets
  cluster_endpoint_public_access = true
  cluster_endpoint_private_access = true

  cluster_addons = {
    coredns = {
      most_recent = true
    }
    kube-proxy = {
      most_recent = true
    }
    vpc-cni = {
      most_recent = true
    }
    aws-ebs-csi-driver = {
      most_recent = true
    }
  }

  # EKS Managed Node Groups
  eks_managed_node_groups = {
    # General purpose nodes
    general = {
      name = "general"
      
      instance_types = ["t3.medium"]
      
      min_size     = 2
      max_size     = 10
      desired_size = 3

      disk_size = 50
      disk_type = "gp3"

      labels = {
        role = "general"
      }

      update_config = {
        max_unavailable_percentage = 33
      }

      tags = {
        ExtraTag = "general-nodes"
      }
    }

    # Compute optimized nodes for data processing
    compute = {
      name = "compute"
      
      instance_types = ["c5.large", "c5.xlarge"]
      
      min_size     = 0
      max_size     = 5
      desired_size = 1

      disk_size = 100
      disk_type = "gp3"

      labels = {
        role = "compute"
      }

      taints = {
        dedicated = {
          key    = "compute"
          value  = "true"
          effect = "NO_SCHEDULE"
        }
      }

      tags = {
        ExtraTag = "compute-nodes"
      }
    }

    # Memory optimized nodes for caching and analytics
    memory = {
      name = "memory"
      
      instance_types = ["r5.large", "r5.xlarge"]
      
      min_size     = 0
      max_size     = 3
      desired_size = 1

      disk_size = 100
      disk_type = "gp3"

      labels = {
        role = "memory"
      }

      taints = {
        dedicated = {
          key    = "memory"
          value  = "true"
          effect = "NO_SCHEDULE"
        }
      }

      tags = {
        ExtraTag = "memory-nodes"
      }
    }
  }

  # Cluster security group additional rules
  cluster_security_group_additional_rules = {
    ingress_nodes_ephemeral_ports_tcp = {
      description                = "Nodes on ephemeral ports"
      protocol                   = "tcp"
      from_port                  = 1025
      to_port                    = 65535
      type                       = "ingress"
      source_node_security_group = true
    }
  }

  # Node security group additional rules
  node_security_group_additional_rules = {
    ingress_self_all = {
      description = "Node to node all ports/protocols"
      protocol    = "-1"
      from_port   = 0
      to_port     = 0
      type        = "ingress"
      self        = true
    }
  }

  # aws-auth configmap
  manage_aws_auth_configmap = true

  aws_auth_roles = [
    {
      rolearn  = aws_iam_role.eks_admin.arn
      username = "eks-admin"
      groups   = ["system:masters"]
    },
  ]

  aws_auth_users = [
    {
      userarn  = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:user/devops"
      username = "devops"
      groups   = ["system:masters"]
    },
  ]

  tags = local.common_tags
}

# EKS Admin Role
resource "aws_iam_role" "eks_admin" {
  name = "${local.name_prefix}-eks-admin"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          AWS = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:root"
        }
      }
    ]
  })

  tags = local.common_tags
}

# Kubernetes provider configuration
provider "kubernetes" {
  host                   = module.eks.cluster_endpoint
  cluster_ca_certificate = base64decode(module.eks.cluster_certificate_authority_data)

  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = "aws"
    args        = ["eks", "get-token", "--cluster-name", module.eks.cluster_name]
  }
}

provider "helm" {
  kubernetes {
    host                   = module.eks.cluster_endpoint
    cluster_ca_certificate = base64decode(module.eks.cluster_certificate_authority_data)

    exec {
      api_version = "client.authentication.k8s.io/v1beta1"
      command     = "aws"
      args        = ["eks", "get-token", "--cluster-name", module.eks.cluster_name]
    }
  }
}

