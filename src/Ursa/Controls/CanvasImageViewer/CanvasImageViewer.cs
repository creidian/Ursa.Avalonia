using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Ursa.Controls;

[TemplatePart(PART_Canvas, typeof(Image))]
[TemplatePart(PART_Layer, typeof(VisualLayerManager))]
[PseudoClasses(PC_Moving)]
public class CanvasImageViewer : TemplatedControl
{
    public const string PART_Canvas = "PART_Canvas";
    public const string PART_Layer = "PART_Layer";
    public const string PC_Moving = ":moving";

    private Canvas? _canvas;
    private ImageBrush? _canvasBK;
    private Point? _lastClickPoint;
    private Point? _lastLocation;
    private bool _moving;

    public static readonly StyledProperty<Control?> OverlayerProperty = AvaloniaProperty.Register<CanvasImageViewer, Control?>(
        nameof(Overlayer));

    public Control? Overlayer
    {
        get => GetValue(OverlayerProperty);
        set => SetValue(OverlayerProperty, value);
    }

    public static readonly StyledProperty<IImageBrushSource?> SourceProperty = ImageBrush.SourceProperty.AddOwner<CanvasImageViewer>();
    public IImageBrushSource? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private double _scale  = 1;

    public static readonly DirectProperty<CanvasImageViewer, double> ScaleProperty = AvaloniaProperty.RegisterDirect<CanvasImageViewer, double>(
        nameof(Scale), o => o.Scale, (o,v)=> o.Scale = v, unsetValue: 1);

    public double Scale
    {
        get => _scale;
        set => SetAndRaise(ScaleProperty, ref _scale, value);
    }

    public static readonly DirectProperty<CanvasImageViewer, double> MinScaleProperty = AvaloniaProperty.RegisterDirect<CanvasImageViewer, double>(
        nameof(MinScale), o => o.MinScale, (o, v) => o.MinScale = v, unsetValue: 0.1);

    public double MinScale
    {
        get => _minScale;
        set => SetAndRaise(MinScaleProperty, ref _minScale, value);
    }
    private double _minScale = 1;

    public static readonly DirectProperty<CanvasImageViewer, double> MaxScaleProperty = AvaloniaProperty.RegisterDirect<CanvasImageViewer, double>(
        nameof(MaxScale), o => o.MaxScale, (o, v) => o.MaxScale = v, unsetValue: 0.1);

    public double MaxScale
    {
        get => _maxScale;
        set => SetAndRaise(MaxScaleProperty, ref _maxScale, value);
    }
    private double _maxScale = 1;

    private double _translateX;

    public static readonly DirectProperty<CanvasImageViewer, double> TranslateXProperty = AvaloniaProperty.RegisterDirect<CanvasImageViewer, double>(
        nameof(TranslateX), o => o.TranslateX, (o,v)=>o.TranslateX = v, unsetValue: 0);

    public double TranslateX
    {
        get => _translateX;
        set => SetAndRaise(TranslateXProperty, ref _translateX, value);
    }

    private double _translateY;

    public static readonly DirectProperty<CanvasImageViewer, double> TranslateYProperty =
        AvaloniaProperty.RegisterDirect<CanvasImageViewer, double>(
            nameof(TranslateY), o => o.TranslateY, (o, v) => o.TranslateY = v, unsetValue: 0);

    public double TranslateY
    {
        get => _translateY;
        set => SetAndRaise(TranslateYProperty, ref _translateY, value);
    }

    public static readonly StyledProperty<double> SmallChangeProperty = AvaloniaProperty.Register<CanvasImageViewer, double>(
        nameof(SmallChange), defaultValue: 1);

    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public static readonly StyledProperty<double> LargeChangeProperty = AvaloniaProperty.Register<CanvasImageViewer, double>(
        nameof(LargeChange), defaultValue: 10);

    public double LargeChange
    {
        get => GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    public static readonly StyledProperty<Stretch> StretchProperty =
        Image.StretchProperty.AddOwner<CanvasImageViewer>(new StyledPropertyMetadata<Stretch>(Stretch.Uniform));

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    private double _sourceMinScale = 0.1;
    private double _sourceMaxScale = -1;

    static CanvasImageViewer()
    {
        FocusableProperty.OverrideDefaultValue<CanvasImageViewer>(true);
        OverlayerProperty.Changed.AddClassHandler<CanvasImageViewer>((o, e) => o.OnOverlayerChanged(e));
        SourceProperty.Changed.AddClassHandler<CanvasImageViewer>((o, e) => o.OnSourceChanged(e));
        TranslateXProperty.Changed.AddClassHandler<CanvasImageViewer>((o,e)=>o.OnTranslateXChanged(e));
        TranslateYProperty.Changed.AddClassHandler<CanvasImageViewer>((o, e) => o.OnTranslateYChanged(e));
        StretchProperty.Changed.AddClassHandler<CanvasImageViewer>((o, e) => o.OnStretchChanged(e));
        MinScaleProperty.Changed.AddClassHandler<CanvasImageViewer>((o, e) => o.OnMinScaleChanged(e));
        MaxScaleProperty.Changed.AddClassHandler<CanvasImageViewer>((o, e) => o.OnMaxScaleChanged(e));
    }

    private void OnTranslateYChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_moving) return;
        var newValue = args.GetNewValue<double>();
        _lastLocation = _lastLocation?.WithY(newValue) ?? new Point(0, newValue);
    }

    private void OnTranslateXChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_moving) return;
        var newValue = args.GetNewValue<double>();
        _lastLocation = _lastLocation?.WithX(newValue) ?? new Point(newValue, 0);
    }

    private void OnOverlayerChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var control = args.GetNewValue<Control?>();
        if (control is { } c)
        {
            AdornerLayer.SetAdorner(this, c);
        }
    }

    private void OnSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_canvasBK is not null)
        {
            IImageBrushSource image = args.GetNewValue<IImageBrushSource>();
            _canvasBK.Source = image;
        }
        //if(!IsLoaded) return;
        //IImage image = args.GetNewValue<IImage>();
        //Size size = image.Size;
        //double width = this.Bounds.Width;
        //double height = this.Bounds.Height;
        //if (_canvas is not null)
        //{
        //    _canvas.Width = size.Width;
        //    _canvas.Height = size.Height;
        //}
        //Scale = GetScaleRatio(width/size.Width, height/size.Height, this.Stretch);
        //_sourceMinScale = Math.Min(width * MinScale / size.Width, height * MinScale / size.Height);
        //_sourceMaxScale = Math.Max(width * MaxScale / size.Width, height * MaxScale / size.Height);
    }

    private void OnStretchChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_canvas is null) return;
        var stretch = args.GetNewValue<Stretch>();
        if (_canvasBK is not null)
        {
            _canvasBK.Stretch = stretch;
        }
        Scale = GetScaleRatio(Width / _canvas!.Width, Height / _canvas!.Height, stretch);
        _sourceMinScale = _canvas is not null ? Math.Min(Width * MinScale / _canvas.Width, Height * MinScale / _canvas.Height) : MinScale;
        _sourceMaxScale = _canvas is not null ? Math.Max(Width * MaxScale / _canvas.Width, Height * MaxScale / _canvas.Height) : MaxScale;
    }

    private void OnMinScaleChanged(AvaloniaPropertyChangedEventArgs _)
    {
        _sourceMinScale = _canvas is not null ? Math.Min(Width * MinScale / _canvas.Width, Height * MinScale / _canvas.Height) : MinScale;
        if (_sourceMinScale > Scale)
        {
            Scale = _sourceMinScale;
        }
    }

    private void OnMaxScaleChanged(AvaloniaPropertyChangedEventArgs _)
    {
        _sourceMaxScale = _canvas is not null ? Math.Max(Width * MaxScale / _canvas.Width, Height * MaxScale / _canvas.Height) : MaxScale;
        if (Scale > _sourceMaxScale)
        {
            Scale = _sourceMaxScale;
        }
    }

    private double GetScaleRatio(double widthRatio, double heightRatio, Stretch stretch)
    {
        return stretch switch
        {
            Stretch.Fill => 1d,
            Stretch.None => 1d,
            Stretch.Uniform => Math.Min(widthRatio, heightRatio),
            Stretch.UniformToFill => Math.Max(widthRatio, heightRatio),
            _ => 1d,
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _canvas = e.NameScope.Get<Canvas>(PART_Canvas);
        e.NameScope.Get<VisualLayerManager>(PART_Layer);
        if (Overlayer is { } c)
        {
            AdornerLayer.SetAdorner(this, c);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (/*Source is { } i && */_canvas is { })
        {
            //Size size = i.Size;
            double width = Bounds.Width;
            double height = Bounds.Height;
            _canvas.Width = width;//size.Width;
            _canvas.Height = height;//size.Height;
            _canvas.Background = _canvasBK = new ImageBrush(Source) { Stretch = this.Stretch};
            Scale = GetScaleRatio(1, 1, this.Stretch);
            _sourceMinScale = Math.Min(width * MinScale / width, height * MinScale / height);
            _sourceMaxScale = Math.Max(width * MaxScale / width, height * MaxScale / height);
            //Scale = GetScaleRatio(width/size.Width, height/size.Height, this.Stretch);
            //_sourceMinScale = Math.Min(width * MinScale / size.Width, height * MinScale / size.Height);
            //_sourceMaxScale = Math.Max(width * MaxScale / size.Width, height * MaxScale / size.Height);
        }
        else
        {
            _sourceMinScale = MinScale;
            _sourceMaxScale = MaxScale;
        }
    }


    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if(e.Delta.Y > 0)
        {
            var scale = Scale;
            scale *= 1.1;
            if (scale > _sourceMaxScale) scale = _sourceMaxScale;
            Scale = scale;
        }
        else
        {
            var scale = Scale;
            scale /= 1.1;
            if (scale < _sourceMinScale) scale = _sourceMinScale;
            Scale = scale;
        }
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (Equals(e.Pointer.Captured, this) && _lastClickPoint != null)
        {
            PseudoClasses.Set(PC_Moving, true);
            Point p = e.GetPosition(this);
            double deltaX = p.X - _lastClickPoint.Value.X;
            double deltaY = p.Y - _lastClickPoint.Value.Y;
            TranslateX = deltaX + (_lastLocation?.X ?? 0);
            TranslateY = deltaY + (_lastLocation?.Y ?? 0);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Pointer.Capture(this);
        _lastClickPoint = e.GetPosition(this);
        _moving = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Pointer.Capture(null);
        _lastLocation = new Point(TranslateX, TranslateY);
        PseudoClasses.Set(PC_Moving, false);
        _moving = false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        double step = e.KeyModifiers.HasFlag(KeyModifiers.Control) ? LargeChange : SmallChange;
        switch (e.Key)
        {
            case Key.Left:
                TranslateX -= step;
                break;
            case Key.Right:
                TranslateX += step;
                break;
            case Key.Up:
                TranslateY -= step;
                break;
            case Key.Down:
                TranslateY += step;
                break;
        }
        base.OnKeyDown(e);
    }
}