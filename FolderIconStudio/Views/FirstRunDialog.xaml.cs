using System.Windows;

namespace FolderIconStudio.Views;

public partial class FirstRunDialog : Window
{
    public bool RegisterContextMenu { get; private set; }

    public FirstRunDialog()
    {
        InitializeComponent();
    }

    private void Yes_Click(object sender, RoutedEventArgs e)
    {
        RegisterContextMenu = true;
        DialogResult = true;
        Close();
    }

    private void No_Click(object sender, RoutedEventArgs e)
    {
        RegisterContextMenu = false;
        DialogResult = true;
        Close();
    }
}
