using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using KartChronoWrapper.Models;
using Serilog;
using System.Text;

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
                Log.Error(ex, "GetList exception");
            }

            return objectNames;
        }

        private static int _hack_counter = 0;
        public async Task SaveCurrentSession(List<PilotProfile> data)
        {
            var htmlContent = new HtmlService().SaveCurrentSession(data);
            await SaveCurrentSession(htmlContent);
        }
        public async Task SaveCurrentSession(string htmlContent)
        {
            byte[] metadataBytes = Encoding.UTF8.GetBytes(htmlContent);
            using (var fileTransferUtility = new TransferUtility(_s3Client))
            using (var stream = new MemoryStream(metadataBytes))
            {
                await fileTransferUtility.UploadAsync(
                    stream,
                    _bucketName,
                    $"storage/{DateTime.Today.ToShortDateString()}/Session-{_hack_counter}-{DateTime.Now.ToShortTimeString()}.html");
                _hack_counter++;
            }
        }

        public async Task<string> GetSession(string key)
        {
            var rq = new GetObjectRequest()
            {
                BucketName = _bucketName,
                Key = key,
            };

            using (GetObjectResponse response = await _s3Client.GetObjectAsync(rq))
            {
                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    string contents = reader.ReadToEnd();

                    return contents;
                }
            }
        }
    }
}
