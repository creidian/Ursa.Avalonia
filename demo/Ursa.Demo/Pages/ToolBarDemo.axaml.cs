using Avalonia.Collections;
using Avalonia.Controls;

namespace Ursa.Demo.Pages;

public partial class ToolBarDemo : UserControl
{
    public ToolBarDemo()
    {
        InitializeComponent();
        AvaloniaList<string> items = new();
    }
}