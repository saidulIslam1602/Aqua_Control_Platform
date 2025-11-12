namespace AquaControl.Infrastructure.ExternalServices.AWS;

public sealed class AwsConfiguration
{
    public const string SectionName = "AWS";

    public string Region { get; init; } = "us-east-1";
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string SessionToken { get; init; } = string.Empty;
    public bool UseInstanceProfile { get; init; } = true;

    // RDS Configuration
    public RdsConfiguration Rds { get; init; } = new();
    
    // S3 Configuration
    public S3Configuration S3 { get; init; } = new();
    
    // SQS Configuration
    public SqsConfiguration Sqs { get; init; } = new();
    
    // SNS Configuration
    public SnsConfiguration Sns { get; init; } = new();
    
    // CloudWatch Configuration
    public CloudWatchConfiguration CloudWatch { get; init; } = new();
}

public sealed class RdsConfiguration
{
    public string ConnectionString { get; init; } = string.Empty;
    public string ReadReplicaConnectionString { get; init; } = string.Empty;
    public bool UseConnectionPooling { get; init; } = true;
    public int MaxPoolSize { get; init; } = 100;
}

public sealed class S3Configuration
{
    public string BucketName { get; init; } = string.Empty;
    public string BackupBucketName { get; init; } = string.Empty;
    public string Region { get; init; } = "us-east-1";
    public bool UseServerSideEncryption { get; init; } = true;
}

public sealed class SqsConfiguration
{
    public string EventQueueUrl { get; init; } = string.Empty;
    public string DeadLetterQueueUrl { get; init; } = string.Empty;
    public int VisibilityTimeoutSeconds { get; init; } = 30;
    public int MaxReceiveCount { get; init; } = 3;
}

public sealed class SnsConfiguration
{
    public string AlertTopicArn { get; init; } = string.Empty;
    public string NotificationTopicArn { get; init; } = string.Empty;
}

public sealed class CloudWatchConfiguration
{
    public string LogGroupName { get; init; } = "/aws/aquacontrol";
    public string MetricNamespace { get; init; } = "AquaControl";
    public bool EnableDetailedMonitoring { get; init; } = true;
}

