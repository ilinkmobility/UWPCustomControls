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
        const string FLIP = "Flip";
        const string IMAGE = "image";
        const string SLIDER = "slider";
        const string GRIDSLIDER = "gridSlider";
        const string OPTIONS = "Options";
        const string ROTATE = "Rotate";
        const string ROTATELEFT = "Rotateleft";
        const string ROTATERIGHT = "Rotateright";
        const string FLIPXLEFT = "FlipXLeft";
        const string FLIPXRIGHT = "FlipXRight";
        const string FLIPYTOP = "FlipYTop";
        const string FLIPYBOTTOM = "FlipYBottom";
        const string SCROLLVIEWER = "scrollViewer";
        const string TRANSFORMGROUP = "transformGroup";
        const string COMPOSITETRANSFORM = "compositeTransform";
        const string SCALETRANSFORM = "scaleTransform";
        const string ROTATETRANSFORM = "rotateTransform";

        double defaultWidth;

        double maxWidth;
        bool isZoomed = false;

        double percent;
        Boolean resized = true;
        StackPanel _options;
        Grid _flip;
        StackPanel _rotate;
        StackPanel _rotateleft;
        StackPanel _rotateright;
        StackPanel _flipxleft;
        StackPanel _flipxright;
        StackPanel _flipytop;
        StackPanel _flipybottom;

        Image _image;
        Grid _grid;
        Grid _gridSlider;
        Slider _slider;
        ScrollViewer _scrollViewer;
        Rectangle _lockRectangle;
        TransformGroup _transformGroup;
        CompositeTransform _compositeTransform;
        ScaleTransform _scaleTransform;
        RotateTransform _rotateTransform;

        public double ZoomDuration { get; set; }

        public Color BackgroundColor { get; set; }

        public double BackgroundOpacity { get; set; }

        public bool HasZoomSlider { get; set; }

        public ImageSource Source { get; set; }

        public double MaxZoomLimit { get; set; } = 3;

        public ImageContentDialog()
        {
            DefaultStyleKey = typeof(ImageContentDialog);

            string platformFamily = AnalyticsInfo.VersionInfo.DeviceFamily;
            if (platformFamily.Equals("Windows.Desktop"))
            {
                //For desktop.
                defaultWidth = Window.Current.Bounds.Width / 2;
            }
            else if (platformFamily.Equals("Windows.Mobile"))
            {
                //For mobile.
                defaultWidth = Window.Current.Bounds.Width/1.1;
            }

            //Triggered when window resized.
            Window.Current.SizeChanged += (sender, args) =>
            {                
                if(platformFamily.Equals("Windows.Mobile"))
                {
                    _grid.Width = Window.Current.Bounds.Width;
                    _grid.Height = Window.Current.Bounds.Height;

                    _image.HorizontalAlignment = HorizontalAlignment.Center;
                    _image.VerticalAlignment = VerticalAlignment.Center;
                    _image.Source = Source;

                    if (isZoomed)
                    _image.Width = Window.Current.Bounds.Width;
                    else
                    _image.Width = Window.Current.Bounds.Width / 1.1;

                }
                else if(platformFamily.Equals("Windows.Desktop"))
                {
                    _grid.Width = Window.Current.Bounds.Width;
                    _grid.Height = Window.Current.Bounds.Height;

                    _image.HorizontalAlignment = HorizontalAlignment.Center;
                    _image.VerticalAlignment = VerticalAlignment.Center;
                    _image.Source = Source;

                    _image.Width = (percent / 100) * Window.Current.Bounds.Width;

                    resized = false;

                    _slider.Minimum = Window.Current.Bounds.Width / 2;
                    _slider.Maximum = Window.Current.Bounds.Width;

                    _image.Width = _slider.Value = (percent / 100) * Window.Current.Bounds.Width;                    

                    resized = true;
                }
            };
        }

        protected override void OnApplyTemplate()
        {
            // this is here by default
            base.OnApplyTemplate();        

            _grid = (Grid)GetTemplateChild(GRID);
            if (_grid == null) throw new NullReferenceException();

            _options = (StackPanel)GetTemplateChild(OPTIONS);
            if (_options == null) throw new NullReferenceException();

            _flip = (Grid)GetTemplateChild(FLIP);
            if (_flip == null) throw new NullReferenceException();

            _gridSlider = (Grid)GetTemplateChild(GRIDSLIDER);
            if (_gridSlider == null) throw new NullReferenceException();

            _image = (Image)GetTemplateChild(IMAGE);
            if (_image == null) throw new NullReferenceException();

            _slider = (Slider)GetTemplateChild(SLIDER);
            if (_slider == null) throw new NullReferenceException();

            _scrollViewer = (ScrollViewer)GetTemplateChild(SCROLLVIEWER);
            if (_scrollViewer == null) throw new NullReferenceException();

            _rotate = (StackPanel)GetTemplateChild(ROTATE);
            if (_rotate == null) throw new NullReferenceException();

            _rotateleft = (StackPanel)GetTemplateChild(ROTATELEFT);
            if (_rotateleft == null) throw new NullReferenceException();

            _rotateright = (StackPanel)GetTemplateChild(ROTATERIGHT);
            if (_rotateright == null) throw new NullReferenceException();

            _flipxleft = (StackPanel)GetTemplateChild(FLIPXLEFT);
            if (_flipxleft == null) throw new NullReferenceException();

            _flipxright = (StackPanel)GetTemplateChild(FLIPXRIGHT);
            if (_flipxright == null) throw new NullReferenceException();

            _flipytop = (StackPanel)GetTemplateChild(FLIPYTOP);
            if (_flipytop == null) throw new NullReferenceException();

            _flipybottom = (StackPanel)GetTemplateChild(FLIPYBOTTOM);
            if (_flipybottom == null) throw new NullReferenceException();

            _transformGroup = (TransformGroup)GetTemplateChild(TRANSFORMGROUP);
            if (_transformGroup == null) throw new NullReferenceException();

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
                _rotate.Visibility = Visibility.Visible;
            }
            else if (platformFamily.Equals("Windows.Mobile"))
            {
                //For mobile.
                _gridSlider.Visibility = Visibility.Collapsed;
                _rotate.Visibility = Visibility.Collapsed;

                _flip.Visibility = Visibility.Visible;

                _image.ManipulationDelta += Image_OnManipulationDelta;
            }

            _image.Tapped += (object sender, TappedRoutedEventArgs e) => e.Handled = true;

            //image double tapped.
            _image.DoubleTapped += (object sender, DoubleTappedRoutedEventArgs e) =>
            {
                var doubleTapPoint = e.GetPosition(_image);
                if (isZoomed)
                {
                    if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"))
                    {
                        DoZoom(Window.Current.Bounds.Width/1.1);
                    }
                    else if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
                    {
                        _slider.Value = Window.Current.Bounds.Width / 2;

                    }
                    isZoomed = false;
                }
                else
                {
                    if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"))
                    {
                        DoZoom(Window.Current.Bounds.Width);
                    }
                    else if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
                    {
                            maxWidth = Window.Current.Bounds.Width;
                            _slider.Value = maxWidth;
                    }
                    
                    isZoomed = true;
                }

                _compositeTransform.TranslateX = _compositeTransform.TranslateY = 0;
            };

            //Slider value changed event.
            _slider.ValueChanged += (object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e) =>
            {
                if(resized)
                {
                    if (sender is Slider slider)
                    {
                        DoZoom(slider.Value);
                        //Percentage of image to the window size.
                        percent = (slider.Value / Window.Current.Bounds.Width) * 100;                    

                        if (slider.Value > Window.Current.Bounds.Width / 2)
                        {
                            isZoomed = true;
                        }
                    }
                }               
            };

            _gridSlider.PointerEntered += (object sender, PointerRoutedEventArgs e) => _slider.Visibility = Visibility.Visible;

            _gridSlider.PointerExited += (object sender, PointerRoutedEventArgs e) => _slider.Visibility = Visibility.Collapsed;

            _scrollViewer.Tapped += OnLockRectangleTapped;            

            _rotateleft.Tapped += _rotateleft_Click;

            _rotateright.Tapped += _rotateright_Click;

            _flipxleft.Tapped += _flipx_Click;
            _flipxright.Tapped += _flipx_Click;

            _flipytop.Tapped += _flipy_Click;
            _flipybottom.Tapped += _flipy_Click;

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

        /// <summary>
        /// Flip vertical button click(up/down).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _flipy_Click(object sender, RoutedEventArgs e)
        {
            if (_scaleTransform.ScaleY == -1)
                FlipAnimation(-1, 1, "Y");
            else
                FlipAnimation(1,-1, "Y");

            _image.UpdateLayout();
        }

        /// <summary>
        /// Flip horizintal button click(left/right).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _flipx_Click(object sender, RoutedEventArgs e)
        {
            if (_scaleTransform.ScaleX == -1)
                FlipAnimation(-1, 1,"X");
            else
                FlipAnimation(1,-1,"X");

            _image.UpdateLayout();
        }

        /// <summary>
        /// Rotate left button click method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _rotateleft_Click(object sender, RoutedEventArgs e)
        {
            RotateAnimation(_rotateTransform.Angle, _rotateTransform.Angle-90);
        }

        /// <summary>
        /// Rotate right button click method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _rotateright_Click(object sender, RoutedEventArgs e)
        {
            RotateAnimation(_rotateTransform.Angle, _rotateTransform.Angle+90);
        }

        /// <summary>
        /// Flip animation for image.
        /// </summary>
        /// <param name="Fromscale"></param>
        /// <param name="Toscale"></param>
        /// <param name="Scale"></param>
        void FlipAnimation(double Fromscale, double Toscale,string Scale)
        {
            DoubleAnimation FlipAnimation = new DoubleAnimation();
            FlipAnimation.Duration = new TimeSpan(0, 0, 0, 0, 350);
            FlipAnimation.From = Fromscale;
            FlipAnimation.To = Toscale;

            Storyboard.SetTarget(FlipAnimation, _scaleTransform);
            if(Scale == "X")
            Storyboard.SetTargetProperty(FlipAnimation, "ScaleX");
            else
            Storyboard.SetTargetProperty(FlipAnimation, "ScaleY");

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(FlipAnimation);
            storyboard.Begin();
        }

        /// <summary>
        /// Rotate Animation for image.
        /// </summary>
        /// <param name="Fromangle"></param>
        /// <param name="Toangle"></param>
        void RotateAnimation(double Fromangle, double Toangle)
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation();
            rotationAnimation.Duration = new TimeSpan(0, 0, 0, 0, 350);
            rotationAnimation.From = Fromangle;
            rotationAnimation.To = Toangle;

            Storyboard.SetTarget(rotationAnimation, _rotateTransform);
            Storyboard.SetTargetProperty(rotationAnimation, "Angle");           

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(rotationAnimation);
            storyboard.Begin();           
        }

        /// <summary>
        /// For Pinch and zoom,rotate in winPhone.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            //Zoom.
            _scaleTransform.ScaleX *= e.Delta.Scale;
            _scaleTransform.ScaleY *= e.Delta.Scale;         
        }

        /// <summary>
        /// Zoom animation for image.
        /// </summary>
        /// <param name="to"></param>
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

        /// <summary>
        /// To Hide the image content dialog when tapped outside the image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnLockRectangleTapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
            _lockRectangle.Tapped -= OnLockRectangleTapped;
        }
    }
}
