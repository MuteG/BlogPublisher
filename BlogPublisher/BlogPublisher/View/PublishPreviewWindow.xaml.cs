using System.Windows;

namespace BlogPublisher.View;

public partial class PublishPreviewWindow
{
    public PublishPreviewWindow()
    {
        InitializeComponent();
    }
    
    private void BtnYes_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void BtnNo_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}