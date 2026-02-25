using System.Windows;

namespace FileMapper.UI;

/// <summary>Interaction logic for MainWindow.xaml</summary>
public partial class MainWindow : Window
{
    /// <summary>Initialises the main window.</summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
