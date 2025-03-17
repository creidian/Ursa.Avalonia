using Avalonia.Controls;
using Avalonia.Interactivity;
using Ursa.Controls;

namespace Ursa.CronPickerDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.ListBox1.ItemsSource = new[] { "Item 1", "Item 2", "Item 3" };
        this.ListBox2.ItemsSource = new[] { "Item 1", "Item 2", "Item 3" };
        this.TestButton.Click += TestButton_Click;
        TestCronPicker.CronExpressionParser = new CronExpressionParser();
    }

    private void TestButton_Click(object? sender, RoutedEventArgs e)
    {
        this.TestCronPicker.SelectCronField(CronFieldTypes.Hour);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }
}