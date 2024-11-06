using Amazon.S3;
using Amazon.S3.Model;
using KartChronoWrapper.Models;

namespace KartChronoWrapper.Services
{
    public class S3FilesService : IRemoteFilesService
    {
        private readonly string _bucketName;
        private readonly AmazonS3Client _s3Client;
        private readonly string _tenantId;
        private readonly string _accessKey;
        private readonly string _secretKey;


        public S3FilesService()
        {
            _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME") ?? string.Empty;
            _tenantId = Environment.GetEnvironmentVariable("TENANT_ID") ?? string.Empty;
            _accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY") ?? string.Empty;
            _secretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ?? string.Empty;

            _s3Client = new AmazonS3Client(
                $"{_tenantId}:{_accessKey}",
                _secretKey,
                new AmazonS3Config
                {
                    ServiceURL = "https://s3.cloud.ru", // Конечная точка Cloud.ru
                    ForcePathStyle = true, // Этот параметр может потребоваться для некоторых S3-совместимых провайдеров
                    AuthenticationRegion = "ru-central-1",
                }
            );
        }

        public async Task<IEnumerable<string>> GetList()
        {
            var objectNames = new List<string>();
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = "",
            };

            ListObjectsV2Response response;
            try
            {
                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);
                    foreach (var obj in response.S3Objects)
                        objectNames.Add(obj.Key);
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //LoggerWrapper.Write(LogEventLevel.Error, ex, $"ListObjectsV2Request failed for {_bucketName} bucket, with [{prefix}] prefix");
            }

            return objectNames;
        }

        public Task SaveCurrentSession(List<PilotProfile> htmlContent)
        {
            throw new NotImplementedException();
        }

        public Task<string> WrapToPage(IEnumerable<string> sessions)
        {
            throw new NotImplementedException();
        }
    }
}
