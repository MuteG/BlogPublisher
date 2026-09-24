using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BlogPublisher.Domain;
using BlogPublisher.View;
using NLog;

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

        public async Task SelectAccessKeyAsync(Window parent)
        {
            var topLevel = TopLevel.GetTopLevel(parent);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择密钥文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("密钥文件 (*.csv)")
                    {
                        Patterns = new[] { "*.csv" }
                    },
                    new FilePickerFileType("所有文件 (*.*)")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var path = files[0].TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    _setting.AccessKeyFile = path;
                }
            }
        }

        public async Task SelectLocalBlogDirectoryAsync(Window parent)
        {
            var topLevel = TopLevel.GetTopLevel(parent);
            if (topLevel == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "选择本地博客文件夹",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                var path = folders[0].TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    _setting.LocalBlogDirectory = path;
                }
            }
        }

        public async Task<bool> PublishAsync(Window parent)
        {
            try
            {
                _setting.Save();
                if (_setting.PreviewBeforePublish)
                {
                    var files = _publisher.Judge();
                    if (files.Count == 0)
                    {
                        await MessageBoxWindow.ShowAsync(parent, "没有发现需要发布的文件！", "提示");
                        return false;
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
                        var result = await preview.ShowDialog<bool?>(parent);
                        if (result == true)
                        {
                            _publisher.Publish(files);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    _publisher.Publish();
                }

                await MessageBoxWindow.ShowAsync(parent, "发布完毕。", "成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                await MessageBoxWindow.ShowAsync(parent, "发布失败，请查看日志文件获取详细信息。", "失败");
                return false;
            }
        }

        private void InitLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logDirectory = "Log";
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = Path.Combine(logDirectory, $"{DateTime.Now:yyyyMMdd}.log") };
            
            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            
            // Apply config           
            LogManager.Configuration = config;
        }
    }
}
