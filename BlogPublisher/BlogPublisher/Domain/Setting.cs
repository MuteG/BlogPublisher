using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Amazon.Runtime;
using BlogPublisher.Annotations;
using YamlDotNet.Serialization;

namespace BlogPublisher.Domain
{
    public sealed class Setting : INotifyPropertyChanged
    {
        private static readonly string _configFile;
        private string _accessKeyFile;
        private string _localBlogDirectory;
        private string _s3Region;
        private string _s3BucketName;
        private string _cloudFrontDistributionId;
        private bool _publishChangedFileOnly;
        private static Setting _instance;

        static Setting()
        {
            _configFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"BlogPublisher\app.yml");
        }

        #region 设定项

        /// <summary>
        /// 密钥文件
        /// </summary>
        public string AccessKeyFile
        {
            get => _accessKeyFile;
            set
            {
                if (value == _accessKeyFile) return;
                _accessKeyFile = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 本地博客文件夹
        /// </summary>
        public string LocalBlogDirectory
        {
            get => _localBlogDirectory;
            set
            {
                if (value == _localBlogDirectory) return;
                _localBlogDirectory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// S3区域
        /// </summary>
        public string S3Region
        {
            get => _s3Region;
            set
            {
                if (value == _s3Region) return;
                _s3Region = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// S3存储桶名称
        /// </summary>
        public string S3BucketName
        {
            get => _s3BucketName;
            set
            {
                if (value == _s3BucketName) return;
                _s3BucketName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// CloudFront分配ID
        /// </summary>
        public string CloudFrontDistributionId
        {
            get => _cloudFrontDistributionId;
            set
            {
                if (value == _cloudFrontDistributionId) return;
                _cloudFrontDistributionId = value;
                OnPropertyChanged();
            }
        }

        public BasicAWSCredentials Credentials
        {
            get
            {
                if (!File.Exists(AccessKeyFile))
                {
                    throw new FileNotFoundException("没找到访问密钥文件。");
                }

                try
                {
                    var line = File.ReadAllLines(AccessKeyFile)[1];
                    var fields = line.Split(',');
                    return new BasicAWSCredentials(fields[2], fields[3]);
                }
                catch
                {
                    throw new FileLoadException("访问秘钥文件读取失败，请检查文件内容。");
                }
            }
        }

        public string PublishDirectory
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), $@"BlogPublisher\Publish");
            }
        }

        public string PublishFileHashDictionary
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"BlogPublisher\hash.dic");
            }
        }

        public bool PublishChangedFileOnly
        {
            get => _publishChangedFileOnly;
            set
            {
                _publishChangedFileOnly = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        
        public static Setting GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Setting();
                _instance.DeserializeSetting();
            }
            
            return _instance;
        }

        public void Save()
        {
            var folder = Path.GetDirectoryName(_configFile);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            SerializeSetting(this, _configFile);
        }

        private void DeserializeSetting()
        {
            var setting = DeserializeSetting<Setting>(_configFile);
            AccessKeyFile = setting.AccessKeyFile;
            LocalBlogDirectory = setting.LocalBlogDirectory;
            S3Region = setting.S3Region;
            S3BucketName = setting.S3BucketName;
            CloudFrontDistributionId = setting.CloudFrontDistributionId;
            PublishChangedFileOnly = setting.PublishChangedFileOnly;
        }

        private TSetting DeserializeSetting<TSetting>(string settingFile)
            where TSetting : class
        {
            if (File.Exists(settingFile))
            {
                var deserializer = new DeserializerBuilder()
                    .Build();
                using var stream = new FileStream(settingFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var setting = deserializer.Deserialize<TSetting>(reader);
                return setting;
            }

            return Activator.CreateInstance<TSetting>();
        }

        private void SerializeSetting(object setting, string settingFile)
        {
            var serializer = new SerializerBuilder()
                .Build();
            using var stream = new FileStream(settingFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            serializer.Serialize(writer, setting);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}