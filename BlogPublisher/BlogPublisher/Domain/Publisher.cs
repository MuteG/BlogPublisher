namespace BlogPublisher.Domain
{
    public class Publisher
    {
        public PublishFileCollection Judge()
        {
            var judge = new PublishFileJudge();
            var files = judge.GetPublishFiles();
            return files;
        }

        public void Publish(PublishFileCollection files)
        {
            if (files.Count > 0)
            {
                var setting = Setting.GetInstance();
                var s3 = new S3Client(setting);
                var cloudFront = new CloudFrontClient(setting);
                s3.Upload();
                cloudFront.Invalid(files);
            }
        }

        public void Publish()
        {
            Publish(Judge());
        }
    }
}