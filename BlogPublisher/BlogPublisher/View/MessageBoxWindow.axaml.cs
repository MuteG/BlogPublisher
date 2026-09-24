using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BlogPublisher.View;

public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow()
    {
        InitializeComponent();
    }

    public MessageBoxWindow(string message, string title = "提示") : this()
    {
        Title = title;
        TxtMessage.Text = message;
    }

    private void BtnOk_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public static async Task ShowAsync(Window owner, string message, string title = "提示")
    {
        var msgBox = new MessageBoxWindow(message, title);
        if (owner != null)
        {
            await msgBox.ShowDialog(owner);
        }
        else
        {
            msgBox.Show();
        }
    }
}
