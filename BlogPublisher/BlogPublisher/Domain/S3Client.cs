using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace BlogPublisher.Domain
{
    public class S3Client
    {
        private readonly Setting _setting;
        private readonly AmazonS3Client _client;

        public S3Client(Setting setting)
        {
            _setting = setting;
            var endpoint = RegionEndpoint.GetBySystemName(_setting.S3Region);
            _client = new AmazonS3Client(_setting.Credentials, endpoint);
        }

        public void Upload()
        {
            var directoryTransferUtility = new TransferUtility(_client);
            directoryTransferUtility.UploadDirectory(
                _setting.PublishDirectory,
                _setting.S3BucketName,
                "*.*", SearchOption.AllDirectories);
        }
    }
}