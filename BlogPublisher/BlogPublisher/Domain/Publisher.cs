using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlogPublisher.Domain
{
    public class Publisher
    {
        private readonly S3Client _s3;
        private readonly CloudFrontClient _cloudFront;

        public Publisher()
        {
            var setting = Setting.GetInstance();
            _s3 = new S3Client(setting);
            _cloudFront = new CloudFrontClient(setting);
        }

        public void Publish()
        {
            GeneratePublishDirectory();
            var setting = Setting.GetInstance();
            var publishFiles = Directory.GetFiles(setting.PublishDirectory, "*.*", SearchOption.AllDirectories);
            if (publishFiles.Length > 0)
            {
                _s3.Upload();
                _cloudFront.Invalid();
            }
        }

        private void GeneratePublishDirectory()
        {
            var setting = Setting.GetInstance();
            if (Directory.Exists(setting.PublishDirectory))
            {
                Directory.Delete(setting.PublishDirectory, true);
            }

            Directory.CreateDirectory(setting.PublishDirectory);
            var dict = LoadPublishFileHashDictionary();
            var publishFileCount = 0;
            foreach (var file in Directory.GetFiles(setting.LocalBlogDirectory, "*.*", SearchOption.AllDirectories))
            {
                if (dict.ContainsKey(file))
                {
                    var fileHash = GetHash(file);
                    if (string.CompareOrdinal(dict[file], fileHash) != 0 ||
                        !setting.PublishChangedFileOnly)
                    {
                        dict[file] = fileHash;
                        MoveToPublishDirectory(file);
                        publishFileCount++;
                    }
                }
                else
                {
                    dict.Add(file, GetHash(file));
                    MoveToPublishDirectory(file);
                    publishFileCount++;
                }
            }

            if (publishFileCount > 0)
            {
                SavePublishFileHashDictionary(dict);
            }
        }

        private string GetHash(string file)
        {
            var md5 = MD5.Create();
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                var hash = new StringBuilder();
                foreach (var data in md5.ComputeHash(stream))
                {
                    hash.Append(data.ToString("X2"));
                }
                
                return hash.ToString();
            }
            
        }

        private Dictionary<string, string> LoadPublishFileHashDictionary()
        {
            var dict = new Dictionary<string, string>();
            var setting = Setting.GetInstance();
            if (File.Exists(setting.PublishFileHashDictionary))
            {
                var lines = File.ReadAllLines(setting.PublishFileHashDictionary);
                foreach (var line in lines)
                {
                    var pair = line.Split(',');
                    dict[pair[0]] = pair[1];
                }
            }
            
            return dict;
        }

        private void MoveToPublishDirectory(string file)
        {
            var setting = Setting.GetInstance();
            var publishFile = Path.Combine(setting.PublishDirectory, file.Substring(setting.LocalBlogDirectory.Length + 1));
            var publishFolder = Path.GetDirectoryName(publishFile);
            if (publishFolder != null && !Directory.Exists(publishFolder))
            {
                Directory.CreateDirectory(publishFolder);
            }
            
            File.Copy(file, publishFile, true);
        }

        private void SavePublishFileHashDictionary(Dictionary<string, string> dict)
        {
            var setting = Setting.GetInstance();
            File.WriteAllLines(setting.PublishFileHashDictionary,
                dict.Select(kv => $"{kv.Key},{kv.Value}"),
                Encoding.UTF8);
        }
    }
}