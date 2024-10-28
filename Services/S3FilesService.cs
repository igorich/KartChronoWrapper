using Amazon.S3;
using Amazon.S3.Model;
using KartChronoWrapper.Models;
using System.Text;

namespace KartChronoWrapper.Services
{
    public class S3FilesService : IRemoteFilesService
    {
        private readonly string _accessKey = "82170b8d2b7e2a4526c519e97fc56d3a";
        private readonly string _secretKey = "241967e2d6037a16f38aa33e61347cf2";
        private readonly string _bucketName = "chrono";

        private readonly AmazonS3Client _s3Client;

        public S3FilesService()
        {
            _s3Client = new AmazonS3Client(
                _accessKey,
                _secretKey,
                new AmazonS3Config
                {
                    ServiceURL = "https://s3.cloud.ru",
                    ForcePathStyle = true, // Этот параметр может потребоваться для некоторых S3-совместимых провайдеров
                    SignatureVersion = "4", // Используем подпись версии 4
                    //RegionEndpoint = RegionEndpoint.GetBySystemName("ru-central-1"),
                });

            //_bucketName = ConfigurationManager.AppSettings["BucketName"];
        }

        public async Task SaveCurrentSession(List<PilotProfile> data)
        {
            var htmlContent = new HtmlService().SaveCurrentSession1(data);
            var objectKey = $"{GetCurrentStorageFolder()}/Заезд-{new Random().Next(1, 10)}.html";
            var byteArray = Encoding.UTF8.GetBytes(htmlContent);
            using var stream = new MemoryStream(byteArray);
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = "text/html",
            };

            PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);
        }

        public async Task<IEnumerable<string>> GetList()
        {
            var prefix = GetCurrentStorageFolder();
            /*var listRequest = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                //Prefix = prefix,
            };

            var listResponse = await _s3Client.ListObjectsV2Async(listRequest);

            var list = listResponse.S3Objects
                .Select(i => i.Key.Length > prefix.Length ? i.Key.Substring(prefix.Length) : i.Key)
                .ToList();

            return list;*/

            var objectNames = new List<string>();
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                //Prefix = prefix,
            };

            ListObjectsV2Response response;
            try
            {
                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);

                    foreach (var obj in response.S3Objects)
                    {
                        objectNames.Add(obj.Key);
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);
            }
            catch (Exception ex)
            {
                //LoggerWrapper.Write(LogEventLevel.Error, ex, $"ListObjectsV2Request failed for {_bucketName} bucket, with [{prefix}] prefix");
                Console.WriteLine($"ListObjectsV2Request failed for {_bucketName} bucket, with [{prefix}] prefix");
            }

            var list = objectNames
                .Select(i => i.Length > prefix.Length ? i.Substring(prefix.Length) : i)
                .ToList();

            return list;
        }

        private string GetCurrentStorageFolder()
        {
            var todaysPrefix = $"storage/{DateTime.Today.ToShortDateString()}/";

            return todaysPrefix;
        }
    }
}
