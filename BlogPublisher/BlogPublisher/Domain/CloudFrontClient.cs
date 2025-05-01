using System;
using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;

namespace BlogPublisher.Domain
{
    public class CloudFrontClient
    {
        private readonly Setting _setting;
        private readonly AmazonCloudFrontClient _client;

        public CloudFrontClient(Setting setting)
        {
            _setting = setting;
            var endpoint = RegionEndpoint.GetBySystemName(_setting.S3Region);
            _client = new AmazonCloudFrontClient(_setting.Credentials, endpoint);
        }

        public CreateInvalidationResponse Invalid(PublishFileCollection files)
        {
            var paths = new Paths();
            paths.Items.AddRange(files.GetInvalidationPath());
            paths.Quantity = paths.Items.Count;
            var batch = new InvalidationBatch(paths, DateTime.Now.ToString("yyyyMMddHHmmss"));
            var request = new CreateInvalidationRequest(_setting.CloudFrontDistributionId, batch);
            return _client.CreateInvalidationAsync(request).Result;
        }
    }
}