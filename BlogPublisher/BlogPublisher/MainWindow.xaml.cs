using System;
using System.Windows;
using System.Windows.Forms;
using BlogPublisher.Domain;
using NLog;
using Binding = System.Windows.Data.Binding;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace BlogPublisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Setting _setting;
        private readonly Publisher _publisher;
        private readonly Logger _logger;

        public MainWindow()
        {
            InitializeComponent();
            _setting = Setting.GetInstance();
            _publisher = new Publisher();
            InitLog();
            _logger = LogManager.GetCurrentClassLogger();
            TxtAccessKeyFile.SetBinding(TextBox.TextProperty, new Binding("AccessKeyFile"){Source = _setting});
            TxtLocalBlogDirectory.SetBinding(TextBox.TextProperty, new Binding("LocalBlogDirectory"){Source = _setting});
            TxtS3Region.SetBinding(TextBox.TextProperty, new Binding("S3Region"){Source = _setting});
            TxtS3BucketName.SetBinding(TextBox.TextProperty, new Binding("S3BucketName"){Source = _setting});
            TxtCloudFrontDistributionId.SetBinding(TextBox.TextProperty, new Binding("CloudFrontDistributionId"){Source = _setting});
        }

        private void BtnAccessKeyFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "密钥文件|*.csv"};
            if (openFileDialog.ShowDialog() == true)
            {
                _setting.AccessKeyFile = openFileDialog.FileName;
            }
        }

        private void BtnLocalBlogDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _setting.LocalBlogDirectory = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void BtnPublish_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _setting.Save();
                _publisher.Publish();
                MessageBox.Show("发布完毕。", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                MessageBox.Show("发布失败，请查看日志文件获取详细信息。", "失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InitLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $@"Log\{DateTime.Now:yyyyMMdd}.log" };
            
            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            
            // Apply config           
            LogManager.Configuration = config;
        }
    }
}