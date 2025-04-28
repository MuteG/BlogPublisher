using System.Windows;
using BlogPublisher.ViewModel;

namespace BlogPublisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private MainModel Model => (MainModel)DataContext;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Model.Initialize();
        }

        private void BtnAccessKeyFile_OnClick(object sender, RoutedEventArgs e)
        {
            Model.SelectAccessKey();
        }

        private void BtnLocalBlogDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            Model.SelectLocalBlogDirectory();
        }

        private void BtnPublish_OnClick(object sender, RoutedEventArgs e)
        {
            Model.Publish();
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}