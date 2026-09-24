using Avalonia.Controls;
using Avalonia.Interactivity;
using BlogPublisher.ViewModel;

namespace BlogPublisher.View;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainModel Model => (MainModel)DataContext;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Model?.Initialize();
    }

    private async void BtnAccessKeyFile_OnClick(object sender, RoutedEventArgs e)
    {
        await Model.SelectAccessKeyAsync(this);
    }

    private async void BtnLocalBlogDirectory_OnClick(object sender, RoutedEventArgs e)
    {
        await Model.SelectLocalBlogDirectoryAsync(this);
    }

    private async void BtnPublish_OnClick(object sender, RoutedEventArgs e)
    {
        var published = await Model.PublishAsync(this);
        if (published)
        {
            Close();
        }
    }

    private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
