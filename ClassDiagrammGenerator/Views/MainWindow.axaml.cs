using Avalonia.Controls;

namespace ClassDiagrammGenerator.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }
}
