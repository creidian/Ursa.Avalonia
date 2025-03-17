using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Utilities;

namespace Ursa.Controls;

public class WrapPanelWithSpacing : WrapPanel
{
    public static readonly StyledProperty<double> RowSpacingProperty = AvaloniaProperty.Register<WrapPanelWithSpacing, double>(nameof(RowSpacing), 0);
    public static readonly StyledProperty<double> ColumnSpacingProperty = AvaloniaProperty.Register<WrapPanelWithSpacing, double>(nameof(ColumnSpacing), 0);

    static WrapPanelWithSpacing()
    {
        AffectsMeasure<WrapPanelWithSpacing>(RowSpacingProperty, ColumnSpacingProperty);
    }

    /// <summary>
    /// Gets or sets the spacing between rows.
    /// </summary>
    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between columns.
    /// </summary>
    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        // Size size = base.MeasureOverride(constraint);
        // return size;
        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        var orientation = Orientation;
        var children = Children;
        var curLineSize = new UVSize(orientation); // 当前行的尺寸
        var panelSize = new UVSize(orientation);
        var uvConstraint = new UVSize(orientation, constraint.Width, constraint.Height); // 尺寸限定
        bool itemWidthSet = !double.IsNaN(itemWidth);
        bool itemHeightSet = !double.IsNaN(itemHeight);
        double h_spacing = ColumnSpacing < 0 ? 0 : ColumnSpacing;
        double v_spacing = RowSpacing < 0 ? 0 : RowSpacing;
        var childConstraint = new Size(itemWidthSet ? itemWidth : constraint.Width, itemHeightSet ? itemHeight : constraint.Height);
        double line_cells_spacing = orientation == Orientation.Horizontal? h_spacing : v_spacing; // 添加方向上元素之间的间距
        double lines_spacing = orientation == Orientation.Horizontal? v_spacing : h_spacing; // 两个平行添加方向上的间距
        double curr_line_cells_spacing = 0; // 当前矢量上各元素之间的间距
        double curr_lines_spacing = 0; // 当前添加矢量间间距
        bool isLineAdding = false;
        int curr_lineIndex = -1;
        for (int i = 0, count = children.Count; i < count; i++)
        {
            var child = children[i];
            // Flow passes its own constraint to children
            child.Measure(childConstraint);
            // This is the size of the child in UV space
            var sz = new UVSize(orientation, itemWidthSet ? itemWidth : child.DesiredSize.Width, itemHeightSet ? itemHeight : child.DesiredSize.Height);
            curr_line_cells_spacing = isLineAdding && i - 1 > -1 ? line_cells_spacing : 0;
            if (MathUtilities.GreaterThan(curLineSize.U + sz.U + curr_line_cells_spacing, uvConstraint.U)) // 比较水平宽 -或- 垂直高，超出限定换行或换列
            {
                ++curr_lineIndex; // 换行/列计数
                panelSize.U = Math.Max(curLineSize.U, panelSize.U);
                panelSize.V += (curr_lines_spacing + curLineSize.V); // 垂直方向增宽 - 或 - 水平方向增高（上一行/列的高/宽）
                curr_lines_spacing = curr_lineIndex > 0 ? lines_spacing : 0; // 换行/列间距
                curLineSize = sz;
                if (MathUtilities.GreaterThan(sz.U + curr_line_cells_spacing, uvConstraint.U)) // 当前所追加元素宽度/高度超限（带间隔计算），则单独成行/列
                {
                    panelSize.U = Math.Max(sz.U, panelSize.U);
                    panelSize.V += (sz.V + curr_lines_spacing);
                    curLineSize = new UVSize(orientation); // Start a new line
                    isLineAdding = false; // 标识下一行第一个元素不需要设置间距
                }
                else
                {
                    isLineAdding = true; // 标识下一个元素需要设置间距
                }
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += sz.U; // 增加当前方向的路线长度（水平方向增宽 - 或 - 垂直方向增高），首元素不加间距
                curLineSize.V = Math.Max(sz.V, curLineSize.V);
                if (!isLineAdding)
                {
                    ++curr_lineIndex;
                    isLineAdding = true; // 标识同一行下一个元素需要设置间距
                }
                else
                {
                    curLineSize.U += line_cells_spacing; // 添加间距
                }
            }
        }

        // The last line size, if any should be added
        panelSize.U = Math.Max(curLineSize.U, panelSize.U);
        panelSize.V += curLineSize.V + curr_lines_spacing;

        // Go from UV space to W/H space
        return new Size(panelSize.Width, panelSize.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        var orientation = Orientation;
        var children = Children;
        int firstInLine = 0;
        double accumulatedV = 0;
        double itemU = orientation == Orientation.Horizontal ? itemWidth : itemHeight;
        var curLineSize = new UVSize(orientation);
        var uvFinalSize = new UVSize(orientation, finalSize.Width, finalSize.Height);
        bool itemWidthSet = !double.IsNaN(itemWidth);
        bool itemHeightSet = !double.IsNaN(itemHeight);
        bool useItemU = orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;
        double h_spacing = ColumnSpacing < 0 ? 0 : ColumnSpacing;
        double v_spacing = RowSpacing < 0 ? 0 : RowSpacing;
        int curr_lineIndex = -1;
        bool isLineAdding = false;
        double line_cells_spacing = orientation == Orientation.Horizontal? h_spacing : v_spacing; // 添加方向上元素之间的间距
        double lines_spacing = orientation == Orientation.Horizontal? v_spacing : h_spacing; // 两个平行添加方向上的间距
        double curr_line_cells_spacing = 0; // 当前矢量上各元素之间的间距
        double curr_lines_spacing = 0; // 当前添加矢量间间距
        for (int i = 0, count = children.Count; i < count; i++)
        {
            var child = children[i];
            var sz = new UVSize(orientation, itemWidthSet ? itemWidth : child.DesiredSize.Width, itemHeightSet ? itemHeight : child.DesiredSize.Height);
            curr_line_cells_spacing = isLineAdding && i - 1 > -1 ? line_cells_spacing : 0;
            if (MathUtilities.GreaterThan(curLineSize.U + sz.U + curr_line_cells_spacing, uvFinalSize.U)) // 比较水平宽 -或- 垂直高，超出限定换行或换列
            {
                ++curr_lineIndex;
                accumulatedV += curr_lines_spacing; // 换行加行间距 - 或 - 换列加列间距
                curr_lines_spacing = curr_lineIndex > 0 ? lines_spacing : 0; // 换行/列间距
                ArrangeLine(accumulatedV, curLineSize.V, firstInLine, i, useItemU, itemU); // 配置上一行元素的位置与尺寸
                accumulatedV += curLineSize.V;
                curLineSize = sz;
                if (MathUtilities.GreaterThan(sz.U + curr_line_cells_spacing, uvFinalSize.U)) // 当前所追加元素宽度/高度超限（带间隔计算），则单独成行/列
                {
                    accumulatedV += curr_lines_spacing; // 添加换线间隔
                    ArrangeLine(accumulatedV, sz.V, i, ++i, useItemU, itemU); // Switch to next line which only contain one element
                    accumulatedV += sz.V;
                    curLineSize = new UVSize(orientation);
                    isLineAdding = false; // 标识下一行第一个元素不需要设置间距
                }
                else
                {
                    isLineAdding = true; // 标识下一个元素需要设置间距
                }

                firstInLine = i;
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += sz.U; // 增加当前方向的路线长度（水平方向增宽 - 或 - 垂直方向增高）
                curLineSize.V = Math.Max(sz.V, curLineSize.V); // 水平时高度取当前最大值，垂直时宽度取当前最大值
                if (!isLineAdding)
                {
                    ++curr_lineIndex;
                    isLineAdding = true; // 标识同一行下一个元素需要设置间距
                }
                else
                {
                    curLineSize.U += line_cells_spacing; // 添加间距
                }
            }
        }

        // Arrange the last line, if any
        if (firstInLine < children.Count)
        {
            ArrangeLine(accumulatedV + curr_lines_spacing, curLineSize.V, firstInLine, children.Count, useItemU, itemU);
        }

        return finalSize;
    }

    private void ArrangeLine(double v, double lineV, int start, int end, bool useItemU, double itemU)
    {
        Orientation orientation = Orientation;
        Avalonia.Controls.Controls children = Children;
        double u = 0;
        bool isHorizontal = orientation == Orientation.Horizontal;
        double spacing = isHorizontal ? ColumnSpacing : RowSpacing;
        spacing = spacing < 0 ? 0 : spacing;
        for (int i = start; i < end; i++)
        {
            Control child = children[i];
            UVSize childSize = new UVSize(orientation, child.DesiredSize.Width, child.DesiredSize.Height);
            double layoutSlotU = useItemU ? itemU : childSize.U;
            var bounds = new Rect(
                isHorizontal ? u : v, // 垂直方向X轴坐标一致
                isHorizontal ? v : u, // 水平方向Y轴坐标一致
                isHorizontal ? layoutSlotU : lineV,
                isHorizontal ? lineV : layoutSlotU);
            child.Arrange(bounds);
            u += (layoutSlotU + spacing); // 下一个元素在水平方向的X轴坐标/垂直方向的Y轴坐标
        }
    }
    
    private struct UVSize
    {
        internal UVSize(Orientation orientation, double width, double height)
        {
            U = V = 0d;
            _orientation = orientation;
            Width = width;
            Height = height;
        }

        internal UVSize(Orientation orientation)
        {
            U = V = 0d;
            _orientation = orientation;
        }

        internal double U; // 水平方向的宽度 - 或 - 垂直方向的高度
        internal double V; // 垂直方向的宽度 - 或 - 水平方向的高度
        private Orientation _orientation;

        internal double Width
        {
            get => _orientation == Orientation.Horizontal ? U : V;
            set { if (_orientation == Orientation.Horizontal) U = value; else V = value; }
        }
        internal double Height
        {
            get => _orientation == Orientation.Horizontal ? V : U;
            set { if (_orientation == Orientation.Horizontal) V = value; else U = value; }
        }
    }
}