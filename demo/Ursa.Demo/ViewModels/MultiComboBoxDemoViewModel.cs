using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ursa.Demo.ViewModels;

public class MultiComboBoxDemoViewModel: ObservableObject
{
    private volatile int _count;

    public ObservableCollection<string> Items { get; set; }

    public ObservableCollection<string> SelectedItems { get; set; }

    public ICommand SelectAllCommand => new RelayCommand(() =>
    {
        SelectedItems.Clear();
        foreach (var item in Items)
        {
            SelectedItems.Add(item);
        }
    });

    public ICommand ClearAllCommand => new RelayCommand(() =>
    {
        SelectedItems.Clear();
    });

    public ICommand InvertSelectionCommand => new RelayCommand(() =>
    {
        var selectedItems = new List<string>(SelectedItems);
        SelectedItems.Clear();
        foreach (var item in Items)
        {
            if (!selectedItems.Contains(item))
            {
                SelectedItems.Add(item);
            }
        }
    });

    public ICommand AddSourceItemCommand => new RelayCommand(() =>
    {
        Task.Run(async () =>
        {
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(1000);
                if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        try
                        {
                            Items.Add("New Item"+ _count);
                            ++_count;
                        }
                        catch (System.Exception ex)
                        {
                        }
                    });
                }
                else
                {
                    try
                    {
                        Items.Add("New Item" + _count);
                        ++_count;
                    }
                    catch (System.Exception ex)
                    {
                    }
                }

            }
        });
    });

    public MultiComboBoxDemoViewModel()
    {
        Items = new ObservableCollection<string>()
        {
            "Item 1",
            "Item 2",
            "Item 3",
            "Item 4",
            "Item 5",
            "Item 6",
            "Item 7",
            "Item 8",
            "Illinois",
            "Indiana",
            "Iowa",
            "Kansas",
            "Kentucky",
            "Louisiana",
            "Maine",
            "Maryland",
            "Massachusetts",
            "Michigan",
            "Minnesota",
            "Mississippi",
            "Missouri",
            "Montana",
            "Nebraska",
            "Nevada",
            "New Hampshire",
            "New Jersey",
            "New Mexico",
            "New York",
            "North Carolina",
            "North Dakota",
            "Ohio",
            "Oklahoma",
            "Oregon",
            "Pennsylvania",
            "Rhode Island",
        };
        SelectedItems = new ObservableCollection<string>();
    }
}