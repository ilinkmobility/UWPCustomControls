using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CustomControls
{
    public sealed class RichPasswordBox : Control
    {
        SolidColorBrush GreenColor = new SolidColorBrush(Colors.Green);
        SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);

        PasswordBox _passwordBox;
        StackPanel _validationStackPanel;
        TextBlock _lowercaseTextBlock;
        TextBlock _uppercaseTextBlock;
        TextBlock _numberTextBlock;
        TextBlock _minCharsTextBlock;

        public RichPasswordBox()
        {
            this.DefaultStyleKey = typeof(RichPasswordBox);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _passwordBox = (PasswordBox)GetTemplateChild("PasswordBox");
            if (_passwordBox == null) throw new NullReferenceException();

            _validationStackPanel = (StackPanel)GetTemplateChild("ValidationStackPanel");
            if (_validationStackPanel == null) throw new NullReferenceException();

            _lowercaseTextBlock = (TextBlock)GetTemplateChild("LowercaseTextBlock");
            if (_lowercaseTextBlock == null) throw new NullReferenceException();

            _uppercaseTextBlock = (TextBlock)GetTemplateChild("UppercaseTextBlock");
            if (_uppercaseTextBlock == null) throw new NullReferenceException();

            _numberTextBlock = (TextBlock)GetTemplateChild("NumberTextBlock");
            if (_numberTextBlock == null) throw new NullReferenceException();

            _minCharsTextBlock = (TextBlock)GetTemplateChild("MinCharsTextBlock");
            if (_minCharsTextBlock == null) throw new NullReferenceException();

            if (!string.IsNullOrEmpty(MinimumCharCountText))
            {
                _minCharsTextBlock.Text = MinimumCharCountText;
            }

            _passwordBox.GotFocus += (object sender, RoutedEventArgs e) => _validationStackPanel.Visibility = Visibility.Visible;

            _passwordBox.LostFocus += (object sender, RoutedEventArgs e) => _validationStackPanel.Visibility = Visibility.Collapsed;

            _passwordBox.PasswordChanged += (object sender, RoutedEventArgs e) =>
            {
                Password = _passwordBox.Password;

                if (string.IsNullOrEmpty(Password))
                {
                    _lowercaseTextBlock.Foreground = _uppercaseTextBlock.Foreground = _numberTextBlock.Foreground = _minCharsTextBlock.Foreground = RedColor;
                    IsValidPassword = false;
                }
                else
                {
                    var HasValidLowercase = Password.Any(c => char.IsLower(c));
                    var HasValidUppercase = Password.Any(c => char.IsUpper(c));
                    var HasValidNumber = Password.Any(c => char.IsNumber(c));
                    var HasValidMinimumChars = (Password.Length >= MinimumCharCount);

                    _lowercaseTextBlock.Foreground = (HasValidLowercase) ? GreenColor : RedColor;
                    _uppercaseTextBlock.Foreground = (HasValidUppercase) ? GreenColor : RedColor;
                    _numberTextBlock.Foreground = (HasValidNumber) ? GreenColor : RedColor;
                    _minCharsTextBlock.Foreground = (HasValidMinimumChars) ? GreenColor : RedColor;

                    IsValidPassword = (HasValidLowercase && HasValidUppercase && HasValidNumber && HasValidMinimumChars);
                }
            };
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(RichPasswordBox), new PropertyMetadata(null));
        
        public bool IsValidPassword
        {
            get { return (bool)GetValue(IsValidPasswordProperty); }
            set { SetValue(IsValidPasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsValidPassword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidPasswordProperty =
            DependencyProperty.Register("IsValidPassword", typeof(bool), typeof(RichPasswordBox), new PropertyMetadata(null));
        
        public int MinimumCharCount
        {
            get { return (int)GetValue(MinimumCharCountProperty); }
            set { SetValue(MinimumCharCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumCharCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumCharCountProperty =
            DependencyProperty.Register("MinimumCharCount", typeof(int), typeof(RichPasswordBox), new PropertyMetadata(null));
        
        public string MinimumCharCountText
        {
            get { return (string)GetValue(MinimumCharCountTextProperty); }
            set { SetValue(MinimumCharCountTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumCharCountText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumCharCountTextProperty =
            DependencyProperty.Register("MinimumCharCountText", typeof(string), typeof(RichPasswordBox), new PropertyMetadata(null));
    }
}