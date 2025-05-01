using BlogPublisher.Domain;
using NLog;
using System;
using System.Windows;
using System.Windows.Forms;
using BlogPublisher.View;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace BlogPublisher.ViewModel
{
    public class MainModel
    {
        private readonly Publisher _publisher = new Publisher();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Setting _setting = Setting.GetInstance();

        public Setting Setting => _setting;

        public void Initialize()
        {
            InitLog();
        }

        public void SelectAccessKey()
        {
            var openFileDialog = new OpenFileDialog {Filter = "密钥文件|*.csv"};
            if (openFileDialog.ShowDialog() == true)
            {
                _setting.AccessKeyFile = openFileDialog.FileName;
            }
        }

        public void SelectLocalBlogDirectory()
        {
            using var folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                _setting.LocalBlogDirectory = folderBrowserDialog.SelectedPath;
            }
        }

        public void Publish()
        {
            try
            {
                _setting.Save();
                if (_setting.PreviewBeforePublish)
                {
                    var files = _publisher.Judge();
                    if (files.Count == 0)
                    {
                        MessageBox.Show("没有发现需要发布的文件！");
                        return;
                    }
                    else
                    {
                        var preview = new PublishPreviewWindow
                        {
                            DataContext = new PublishPreviewModel
                            {
                                PublishPaths = string.Join(Environment.NewLine, files.GetInvalidationPath())
                            }
                        };
                        if (preview.ShowDialog()!.Value)
                        {
                            _publisher.Publish(files);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    _publisher.Publish();
                }

                MessageBox.Show("发布完毕。", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                MessageBox.Show("发布失败，请查看日志文件获取详细信息。", "失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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