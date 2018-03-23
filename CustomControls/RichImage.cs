﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Animation;

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
            DependencyProperty.Register("HasZoomSlider", typeof(bool), typeof(RichImage), new PropertyMetadata(0));

        
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

            _image.Tapped += async (object sender, TappedRoutedEventArgs e) => {
                await new ImageContentDialog() { Source = Source, HasZoomSlider = HasZoomSlider }.ShowAsync();
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
        const string ENTERSTORYBOARD = "enterStoryboard";
        const string EXITSTORYBOARD = "exitStoryboard";
        const string COMPOSITETRANSFORM = "compositeTransform";

        double defaultWidth;

        Image _image;
        Grid _grid;
        Grid _gridSlider;
        Slider _slider;
        Storyboard _enterStoryboard;
        Storyboard _exitStoryboard;
        ScrollViewer _scrollViewer;
        CompositeTransform _compositeTransform;

        public bool HasZoomSlider { get; set; }
        
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageContentDialog), new PropertyMetadata(0));
        
        public ImageContentDialog()
        {
            this.DefaultStyleKey = typeof(ImageContentDialog);

            defaultWidth = Window.Current.Bounds.Width / 2;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _grid = (Grid)GetTemplateChild(GRID);
            if (_grid == null) throw new NullReferenceException();

            _gridSlider = (Grid)GetTemplateChild(GRIDSLIDER);
            if (_gridSlider == null) throw new NullReferenceException();

            _image = (Image)GetTemplateChild(IMAGE);
            if (_image == null) throw new NullReferenceException();

            _slider = (Slider)GetTemplateChild(SLIDER);
            if (_slider == null) throw new NullReferenceException();

            _enterStoryboard = (Storyboard)GetTemplateChild(ENTERSTORYBOARD);
            if (_enterStoryboard == null) throw new NullReferenceException();

            _exitStoryboard = (Storyboard)GetTemplateChild(EXITSTORYBOARD);
            if (_exitStoryboard == null) throw new NullReferenceException();

            _scrollViewer = (ScrollViewer)GetTemplateChild(SCROLLVIEWER);
            if (_scrollViewer == null) throw new NullReferenceException();

            _compositeTransform = (CompositeTransform)GetTemplateChild(COMPOSITETRANSFORM);
            if (_compositeTransform == null) throw new NullReferenceException();

            _image.Source = Source;
            _image.Width = defaultWidth;

            _slider.Minimum = 1;
            _slider.Maximum = 100;

            _grid.Width = Window.Current.Bounds.Width;
            _grid.Height = Window.Current.Bounds.Height;

            _gridSlider.Visibility = (HasZoomSlider) ? Visibility.Visible : Visibility.Collapsed;

            //Check for desktop device using mouse input support
            if (new Windows.Devices.Input.MouseCapabilities().MousePresent == 0)
            {
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
                if (Math.Round(_scrollViewer.ZoomFactor, 1) <= 1)
                { _slider.Value = 100; }
                else
                { _slider.Value = 1; }

                OnZoomUpdate();
            };

            _image.PointerWheelChanged += image_PointerWheelChanged;

            _slider.ValueChanged += (object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e) =>
            {
                OnZoomUpdate();                
            };

            _gridSlider.PointerEntered += (object sender, PointerRoutedEventArgs e) => _enterStoryboard.Begin();

            _gridSlider.PointerExited += (object sender, PointerRoutedEventArgs e) => _exitStoryboard.Begin();

            _scrollViewer.Tapped += OnLockRectangleTapped;        
        }

        /// <summary>
        /// To zoom in and out using slider.
        /// </summary>
        void OnZoomUpdate()
        {
            Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _scrollViewer.ChangeView(_scrollViewer.HorizontalOffset * 2, _scrollViewer.VerticalOffset * 2, (float?)(1 + (_slider.Value / 100)));
                });
            }, TimeSpan.FromMilliseconds(10));
        }

        /// <summary>
        /// To hide the background rectangle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnLockRectangleTapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
        }

        private void image_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            double dblDelta_Scroll = -1 * e.GetCurrentPoint(_image).Properties.MouseWheelDelta; // * + - 120 Mouse
            double posX = e.GetCurrentPoint(_image).Position.X;
            double posY = e.GetCurrentPoint(_image).Position.Y;
            double actWidth = _image.ActualWidth;  // * pixel of real image
            double actHeight = _image.ActualHeight; // * pixel of real image

            dblDelta_Scroll = (dblDelta_Scroll > 0) ? 0.8 : 1.2;

            if (dblDelta_Scroll == 1.2)
            {
                if (_slider.Value != 100)
                {
                    MouseWheel(dblDelta_Scroll, posX, posY);
                    _slider.Value = _slider.Value + 20;
                }
            }
            else
            {
                if (_slider.Value != 1)
                {
                    MouseWheel(dblDelta_Scroll, posX, posY);
                    _slider.Value = _slider.Value - 20;
                }
            }
        }

        private void MouseWheel(double dblDelta_Scroll, double posX, double posY)
        {
            double new_ScaleX = _compositeTransform.ScaleX * dblDelta_Scroll;
            double new_ScaleY = _compositeTransform.ScaleY * dblDelta_Scroll;


            double new_TranslateX = (dblDelta_Scroll > 1) ? (_compositeTransform.TranslateX - (posX * 0.2 * _compositeTransform.ScaleX)) : (_compositeTransform.TranslateX - (posX * -0.2 * _compositeTransform.ScaleX));
            double new_TranslateY = (dblDelta_Scroll > 1) ? (_compositeTransform.TranslateY - (posY * 0.2 * _compositeTransform.ScaleY)) : (_compositeTransform.TranslateY - (posY * -0.2 * _compositeTransform.ScaleY));

            if (new_ScaleX <= 1 | new_ScaleY <= 1) { new_ScaleX = 1; new_ScaleY = 1; new_TranslateX = 0; new_TranslateY = 0; }

            _compositeTransform.ScaleX = new_ScaleX;
            _compositeTransform.ScaleY = new_ScaleY;
            _compositeTransform.TranslateX = new_TranslateX;
            _compositeTransform.TranslateY = new_TranslateY;
        }
    }
}
