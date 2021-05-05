using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using Amazon.Runtime;
using BlogPublisher.Annotations;

namespace BlogPublisher.Domain
{
    public sealed class Setting : INotifyPropertyChanged
    {
        private readonly Configuration _configuration;
        private string _accessKeyFile;
        private string _localBlogDirectory;
        private string _s3Region;
        private string _s3BucketName;
        private string _cloudFrontDistributionId;
        private static Setting _instance;

        private Setting()
        {
            var configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"BlogPublisher\app.config");
            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configFile
            };
            
            _configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            AccessKeyFile = _configuration.AppSettings.Settings["AccessKeyFile"]?.Value;
            LocalBlogDirectory = _configuration.AppSettings.Settings["LocalBlogDirectory"]?.Value;
            S3Region = _configuration.AppSettings.Settings["S3Region"]?.Value;
            S3BucketName = _configuration.AppSettings.Settings["S3BucketName"]?.Value;
            CloudFrontDistributionId = _configuration.AppSettings.Settings["CloudFrontDistributionId"]?.Value;
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        
        public static Setting GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Setting();
            }
            
            return _instance;
        }

        public void Save()
        {
            var folder = Path.GetDirectoryName(_configuration.FilePath);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            _configuration.AppSettings.Settings.Clear();
            _configuration.AppSettings.Settings.Add("AccessKeyFile", AccessKeyFile);
            _configuration.AppSettings.Settings.Add("LocalBlogDirectory", LocalBlogDirectory);
            _configuration.AppSettings.Settings.Add("S3Region", S3Region);
            _configuration.AppSettings.Settings.Add("S3BucketName", S3BucketName);
            _configuration.AppSettings.Settings.Add("CloudFrontDistributionId", CloudFrontDistributionId);
            _configuration.Save(ConfigurationSaveMode.Modified);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}