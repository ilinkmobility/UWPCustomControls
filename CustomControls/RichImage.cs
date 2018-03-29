using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Animation;
using Windows.System.Profile;
using Windows.Foundation;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CustomControls
{
    public sealed class RichImage : Control
    {
        const string IMAGE = "image";

        private Image _image;

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(RichImage), new PropertyMetadata(0));

        public bool HasZoomSlider
        {
            get { return (bool)GetValue(HasZoomSliderProperty); }
            set { SetValue(HasZoomSliderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasZoomSlider.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasZoomSliderProperty =
            DependencyProperty.Register("HasZoomSlider", typeof(bool), typeof(RichImage), new PropertyMetadata(false));

        public double ZoomDuration
        {
            get { return (double)GetValue(ZoomDurationProperty); }
            set { SetValue(ZoomDurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomDurationProperty =
            DependencyProperty.Register("ZoomDuration", typeof(double), typeof(RichImage), new PropertyMetadata(200.0));

        public double BackgroundOpacity
        {
            get { return (double)GetValue(BackgroundOpacityProperty); }
            set { SetValue(BackgroundOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundOpacityProperty =
            DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(RichImage), new PropertyMetadata(0.5));
        
        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(RichImage), new PropertyMetadata(Colors.Black));

        public RichImage()
        {
            this.DefaultStyleKey = typeof(RichImage);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _image = (Image)GetTemplateChild(IMAGE);
            if (_image == null) throw new NullReferenceException();

            _image.Source = Source;

            _image.Tapped += async (object sender, TappedRoutedEventArgs e) =>
            {
                await new ImageContentDialog() { Source = Source, HasZoomSlider = HasZoomSlider, ZoomDuration = ZoomDuration, BackgroundOpacity = BackgroundOpacity, BackgroundColor = BackgroundColor }.ShowAsync();
            };
        }
    }

    public class ImageContentDialog : ContentDialog
    {
        const string GRID = "grid";
        const string IMAGE = "image";
        const string SLIDER = "slider";
        const string GRIDSLIDER = "gridSlider";
        const string SCROLLVIEWER = "scrollViewer";
        const string COMPOSITETRANSFORM = "compositeTransform";
        const string SCALETRANSFORM = "scaleTransform";
        const string ROTATETRANSFORM = "rotateTransform";

        double defaultWidth;

        double maxWidth;
        bool isZoomed;

        Image _image;
        Grid _grid;
        Grid _gridSlider;
        Slider _slider;
        ScrollViewer _scrollViewer;
        Rectangle _lockRectangle;
        CompositeTransform _compositeTransform;
        ScaleTransform _scaleTransform;
        RotateTransform _rotateTransform;

        public double ZoomDuration { get; set; }

        public Color BackgroundColor { get; set; }

        public double BackgroundOpacity { get; set; }

        public bool HasZoomSlider { get; set; }

        public ImageSource Source { get; set; }

        public ImageContentDialog()
        {
            DefaultStyleKey = typeof(ImageContentDialog);

            defaultWidth = Window.Current.Bounds.Width/2;
        }

        protected override void OnApplyTemplate()
        {
            // this is here by default
            base.OnApplyTemplate();        

            _grid = (Grid)GetTemplateChild(GRID);
            if (_grid == null) throw new NullReferenceException();

            _gridSlider = (Grid)GetTemplateChild(GRIDSLIDER);
            if (_gridSlider == null) throw new NullReferenceException();

            _image = (Image)GetTemplateChild(IMAGE);
            if (_image == null) throw new NullReferenceException();

            _slider = (Slider)GetTemplateChild(SLIDER);
            if (_slider == null) throw new NullReferenceException();

            _scrollViewer = (ScrollViewer)GetTemplateChild(SCROLLVIEWER);
            if (_scrollViewer == null) throw new NullReferenceException();            

            _compositeTransform = (CompositeTransform)GetTemplateChild(COMPOSITETRANSFORM);
            if (_compositeTransform == null) throw new NullReferenceException();

            _scaleTransform = (ScaleTransform)GetTemplateChild(SCALETRANSFORM);
            if (_scaleTransform == null) throw new NullReferenceException();

            _rotateTransform = (RotateTransform)GetTemplateChild(ROTATETRANSFORM);
            if (_rotateTransform == null) throw new NullReferenceException();

            _image.Source = Source;
            _image.Width = defaultWidth;

            _slider.Minimum = defaultWidth;
            _slider.Maximum = Window.Current.Bounds.Width;

            _grid.Width = Window.Current.Bounds.Width;
            _grid.Height = Window.Current.Bounds.Height;

            _gridSlider.Visibility = (HasZoomSlider) ? Visibility.Visible : Visibility.Collapsed;

            //Check for desktop device using mouse input support.
            string platformFamily = AnalyticsInfo.VersionInfo.DeviceFamily;
            if (platformFamily.Equals("Windows.Desktop"))
            {
                //For desktop.
                _gridSlider.Visibility = Visibility.Visible;
            }
            else if (platformFamily.Equals("Windows.Mobile"))
            {
                //For mobile.
                _gridSlider.Visibility = Visibility.Collapsed;
            }

            _image.ManipulationDelta += (object sender, ManipulationDeltaRoutedEventArgs e) =>
            {
                _compositeTransform.TranslateX += e.Delta.Translation.X;
                _compositeTransform.TranslateY += e.Delta.Translation.Y;
            };

            _image.Tapped += (object sender, TappedRoutedEventArgs e) => e.Handled = true;

            _image.DoubleTapped += (object sender, DoubleTappedRoutedEventArgs e) =>
            {
                if (isZoomed)
                {
                    _slider.Value = defaultWidth;
                    isZoomed = false;
                }
                else
                {
                    if (maxWidth == 0)
                    {
                        maxWidth = Window.Current.Bounds.Width;
                    }

                    _slider.Value = maxWidth;
                    isZoomed = true;
                }

                _compositeTransform.TranslateX = _compositeTransform.TranslateY = 0;
            };

            _slider.ValueChanged += (object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e) =>
            {
                if (sender is Slider slider)
                {
                    DoZoom(slider.Value);

                    if (slider.Value > defaultWidth)
                    {
                        isZoomed = true;
                    }
                }
            };

            _gridSlider.PointerEntered += (object sender, PointerRoutedEventArgs e) => _slider.Visibility = Visibility.Visible;

            _gridSlider.PointerExited += (object sender, PointerRoutedEventArgs e) => _slider.Visibility = Visibility.Collapsed;

            _scrollViewer.Tapped += OnLockRectangleTapped;

            _image.ManipulationDelta += Image_OnManipulationDelta;

            // get all open popups
            // normally there are 2 popups, one for your ContentDialog and one for Rectangle
            var popups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in popups)
            {
                if (popup.Child is Rectangle)
                {
                    // I store a refrence to Rectangle to be able to unregester event handler later
                    _lockRectangle = popup.Child as Rectangle;
                    _lockRectangle.Opacity = BackgroundOpacity;
                    _lockRectangle.Fill = new SolidColorBrush(BackgroundColor);
                    _lockRectangle.Tapped += OnLockRectangleTapped;
                }
            }
        }

        void Image_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            FrameworkElement origin = sender as FrameworkElement;
            FrameworkElement parent = origin.Parent as FrameworkElement;

            var localCoords = e.Position;
            var relativeTransform = origin.TransformToVisual(parent);
            Point parentContainerCoords = relativeTransform.TransformPoint(localCoords);
            var center = parentContainerCoords;
          
            //Rotate.
            _rotateTransform.Angle += e.Delta.Rotation;
            
            //zoom.
            _scaleTransform.ScaleX *= e.Delta.Scale;
            _scaleTransform.ScaleY *= e.Delta.Scale;
        }        

        async void DoZoom(double to)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = _image.Width,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(ZoomDuration)),
                EnableDependentAnimation = true
            };

            Storyboard.SetTarget(myDoubleAnimation, _image);
            Storyboard.SetTargetProperty(myDoubleAnimation, "Width");
            Storyboard.SetTargetName(myDoubleAnimation, _image.Name);

            Storyboard justintimeStoryboard = new Storyboard();
            justintimeStoryboard.Children.Add(myDoubleAnimation);

            await justintimeStoryboard.BeginAsync();
        }

        void OnLockRectangleTapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
            _lockRectangle.Tapped -= OnLockRectangleTapped;
        }
    }
}
