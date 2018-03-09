using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Shapes;

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
                //await new ImageContentDialog(Source).ShowAsync();
                await new ImageContentDialog() { Source = Source }.ShowAsync();
            };
        }
    }

    public class ImageContentDialog : ContentDialog
    {
        const string GRID = "grid";
        const string IMAGE = "image";
        const string SCROLLVIEWER = "scrollViewer";
        const string COMPOSITETRANSFORM = "compositeTransform";

        const double defaultWidth = 300;

        int defaultX;
        int defaultY;

        double maxWidth;
        bool isZoomed;

        Image _image;
        Grid _grid;
        ScrollViewer _scrollViewer;
        Rectangle _lockRectangle;
        CompositeTransform _compositeTransform;
        
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageContentDialog), new PropertyMetadata(0));
        
        public ImageContentDialog()
        {
            this.DefaultStyleKey = typeof(ImageContentDialog);
        }

        protected override void OnApplyTemplate()
        {
            // this is here by default
            base.OnApplyTemplate();

            _grid = (Grid)GetTemplateChild(GRID);
            if (_grid == null) throw new NullReferenceException();

            _image = (Image)GetTemplateChild(IMAGE);
            if (_image == null) throw new NullReferenceException();

            _scrollViewer = (ScrollViewer)GetTemplateChild(SCROLLVIEWER);
            if (_scrollViewer == null) throw new NullReferenceException();

            _compositeTransform = (CompositeTransform)GetTemplateChild(COMPOSITETRANSFORM);
            if (_compositeTransform == null) throw new NullReferenceException();

            _compositeTransform = (CompositeTransform)GetTemplateChild(COMPOSITETRANSFORM);
            if (_compositeTransform == null) throw new NullReferenceException();

            _image.Source = Source;
            _image.Width = defaultWidth;

            _grid.Width = Window.Current.Bounds.Width;
            _grid.Height = Window.Current.Bounds.Height;

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
                    _image.Width = defaultWidth;
                    isZoomed = false;
                }
                else
                {

                    if (maxWidth == 0)
                    {
                        maxWidth = Window.Current.Bounds.Width - 100;
                    }

                    _image.Width = maxWidth;
                    isZoomed = true;
                }
            };

            _scrollViewer.Tapped += OnLockRectangleTapped;

            // get all open popups
            // normally there are 2 popups, one for your ContentDialog and one for Rectangle
            var popups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in popups)
            {
                if (popup.Child is Rectangle)
                {
                    // I store a refrence to Rectangle to be able to unregester event handler later
                    _lockRectangle = popup.Child as Rectangle;
                    _lockRectangle.Opacity = 0.5f;
                    _lockRectangle.Fill = new SolidColorBrush(Colors.Black);
                    _lockRectangle.Tapped += OnLockRectangleTapped;
                }
            }
        }

        void OnLockRectangleTapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
            _lockRectangle.Tapped -= OnLockRectangleTapped;
        }
    }
}
