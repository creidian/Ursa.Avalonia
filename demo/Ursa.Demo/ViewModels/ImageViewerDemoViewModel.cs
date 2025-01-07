using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Ursa.Demo.ViewModels;

public partial class ImageViewerDemoViewModel: ObservableObject
{
    [ObservableProperty] private Stretch _stretch;
    public Type StrethType { get; } = typeof(Stretch);
}
