using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BlogPublisher.View;

public partial class PublishPreviewWindow : Window
{
    public PublishPreviewWindow()
    {
        InitializeComponent();
    }
    
    private void BtnYes_OnClick(object sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void BtnNo_OnClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
