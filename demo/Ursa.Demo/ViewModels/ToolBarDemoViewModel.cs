using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ursa.Controls;

namespace Ursa.Demo.ViewModels;

public partial class ToolBarDemoViewModel : ObservableObject
{
    public ObservableCollection<ToolBarItemViewModel> Items { get; set; }
    public ToolBarDemoViewModel()
    {
        Items = new()
        {
            new ToolBarButtonItemViewModel() { Content = "New", OverflowMode = OverflowMode.AsNeeded},
            new ToolBarButtonItemViewModel() { Content = "Open" },
            new ToolBarButtonItemViewModel() { Content = "Save1" },
            new ToolBarButtonItemViewModel() { Content = "Save2" },
            new ToolBarSeparatorViewModel(),
            new ToolBarButtonItemViewModel() { Content = "Save3" },
            new ToolBarButtonItemViewModel() { Content = "Save4" },
            new ToolBarButtonItemViewModel() { Content = "Save5" },
            new ToolBarButtonItemViewModel() { Content = "Save6" },
            new ToolBarButtonItemViewModel() { Content = "Save7" },
            new ToolBarSeparatorViewModel(),
            new ToolBarButtonItemViewModel() { Content = "Save8" },
            new ToolBarCheckBoxItemViweModel() { Content = "Bold" },
            new ToolBarCheckBoxItemViweModel() { Content = "Italic", OverflowMode = OverflowMode.Never},
            new ToolBarComboBoxItemViewModel() { Content = "Font Size", Items = new (){ "10", "12", "14"  } }
        };
        AddItemCommand = new RelayCommand(async () =>
        {
            try
            {
                Items.Add(new ToolBarButtonItemViewModel() { Content = "New Item" });
            }
            catch (System.Exception ex)
            {
                await MessageBox.ShowAsync(ex.Message, "Error", MessageBoxIcon.Error);
            }
        });
        RemoveItemCommand = new RelayCommand(async () =>
        {
            try
            {
                int index = Items.Count - 1;
                if (index >= 0 && index < Items.Count)
                {
                    Items.RemoveAt(index);
                }
            }
            catch (System.Exception ex)
            {
                await MessageBox.ShowAsync(ex.Message, "Error", MessageBoxIcon.Error);
            }
        });
        ClearItemsCommand = new RelayCommand(async () =>
        {
            try
            {
                Items.Clear();
            }
            catch (System.Exception ex)
            {
                await MessageBox.ShowAsync(ex.Message, "Error", MessageBoxIcon.Error);
            }
        });
        RevertItemsCommand = new RelayCommand(async () =>
        {
            try
            {
                Items.Clear();
                Items.Add(new ToolBarButtonItemViewModel() { Content = "New", OverflowMode = OverflowMode.AsNeeded });
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Open" });
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save1" });
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save2" });
                Items.Add(new ToolBarSeparatorViewModel());
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save3" });
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save4" });
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save5" });
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save6" });
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save7" });
                Items.Add(new ToolBarSeparatorViewModel());
                Items.Add(new ToolBarButtonItemViewModel() { Content = "Save8" });
                Items.Add(new ToolBarCheckBoxItemViweModel() { Content = "Bold" });
                Items.Add(new ToolBarCheckBoxItemViweModel() { Content = "Italic", OverflowMode = OverflowMode.Never });
                Items.Add(new ToolBarComboBoxItemViewModel() { Content = "Font Size", Items = new() { "10", "12", "14" } });
            }
            catch (System.Exception ex)
            {
                await MessageBox.ShowAsync(ex.Message, "Error", MessageBoxIcon.Error);
            }
        });
    }

    public ICommand AddItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand ClearItemsCommand { get; }
    public ICommand RevertItemsCommand { get; }
}

public abstract class ToolBarItemViewModel: ObservableObject
{
    public OverflowMode OverflowMode { get; set; }
}

public class ToolBarButtonItemViewModel: ToolBarItemViewModel
{
    public string Content { get; set; }
    public ICommand Command { get; set; }

    public ToolBarButtonItemViewModel()
    {
        Command = new AsyncRelayCommand(async () => { await MessageBox.ShowOverlayAsync(Content); });
    }
}

public class ToolBarCheckBoxItemViweModel: ToolBarItemViewModel
{
    public string Content { get; set; }
    public bool IsChecked { get; set; }
    public ICommand Command { get; set; }

    public ToolBarCheckBoxItemViweModel()
    {
        Command = new AsyncRelayCommand(async () => { await MessageBox.ShowOverlayAsync(Content); });
    }
}

public class ToolBarComboBoxItemViewModel: ToolBarItemViewModel
{
    public string Content { get; set; }
    public ObservableCollection<string> Items { get; set; }

    private string _selectedItem;
    public string SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            MessageBox.ShowOverlayAsync(value);
        }
    }
}

public class ToolBarSeparatorViewModel: ToolBarItemViewModel
{

}