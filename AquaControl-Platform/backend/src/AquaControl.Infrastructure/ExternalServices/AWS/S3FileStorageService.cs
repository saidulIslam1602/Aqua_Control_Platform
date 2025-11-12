using Amazon.S3;
using Amazon.S3.Model;
using AquaControl.Application.Common.Interfaces;

namespace AquaControl.Infrastructure.ExternalServices.AWS;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string fileKey, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default);
    Task<string> GeneratePresignedUrlAsync(string fileKey, TimeSpan expiration, CancellationToken cancellationToken = default);
}

public sealed class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsConfiguration _awsConfig;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(
        IAmazonS3 s3Client,
        AwsConfiguration awsConfig,
        ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client;
        _awsConfig = awsConfig;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var fileKey = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}/{fileName}";

        _logger.LogInformation("Uploading file to S3: {FileKey}", fileKey);

        var request = new PutObjectRequest
        {
            BucketName = _awsConfig.S3.BucketName,
            Key = fileKey,
            InputStream = fileStream,
            ContentType = contentType,
            ServerSideEncryptionMethod = _awsConfig.S3.UseServerSideEncryption 
                ? ServerSideEncryptionMethod.AES256 
                : ServerSideEncryptionMethod.None,
            Metadata =
            {
                ["uploaded-by"] = "aquacontrol-platform",
                ["uploaded-at"] = DateTime.UtcNow.ToString("O")
            }
        };

        try
        {
            var response = await _s3Client.PutObjectAsync(request, cancellationToken);
            
            _logger.LogInformation("File uploaded successfully to S3: {FileKey}, ETag: {ETag}", 
                fileKey, response.ETag);

            return fileKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to S3: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading file from S3: {FileKey}", fileKey);

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _awsConfig.S3.BucketName,
                Key = fileKey
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            
            _logger.LogInformation("File downloaded successfully from S3: {FileKey}", fileKey);
            
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from S3: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file from S3: {FileKey}", fileKey);

        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _awsConfig.S3.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            
            _logger.LogInformation("File deleted successfully from S3: {FileKey}", fileKey);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from S3: {FileKey}", fileKey);
            return false;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(
        string fileKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating presigned URL for S3 file: {FileKey}", fileKey);

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _awsConfig.S3.BucketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            
            _logger.LogDebug("Presigned URL generated for S3 file: {FileKey}", fileKey);
            
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate presigned URL for S3 file: {FileKey}", fileKey);
            throw;
        }
    }
}

