using Avalonia.Controls;
using Avalonia.Input;

namespace Ursa.Controls;

public interface ISelectableRulerItemContainer
{
    bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs args);
    void OnValueChange(bool isSelected, string rulerValue);
    void OnSelectedStatusChanged(CronPickerRulerSelector cronPickerRulerSelector, bool isSelected, string rulerValue);
}