using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CustomControls
{
    public sealed partial class RichPasswordBox : UserControl
    {
        SolidColorBrush GreenColor = new SolidColorBrush(Colors.Green);
        SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);

        static RichPasswordBox CurrentInstance { get; set; }

        public int MinimumCharCount
        {
            get { return (int)GetValue(MinimumCharCountProperty); }
            set { SetValue(MinimumCharCountProperty, value); }
        }

        public static readonly DependencyProperty MinimumCharCountProperty = DependencyProperty.Register("MinimumCharCount",
            typeof(int),
            typeof(RichPasswordBox),
            new PropertyMetadata(null, new PropertyChangedCallback(OnRichPasswordBoxMinimumCharCountChanged)));

        public string MinimumCharCountDisplayText
        {
            get { return (string)GetValue(MinimumCharCountDisplayTextProperty); }
            set { SetValue(MinimumCharCountDisplayTextProperty, value); }
        }

        public static readonly DependencyProperty MinimumCharCountDisplayTextProperty = DependencyProperty.Register("MinimumCharCountDisplayText",
            typeof(string),
            typeof(RichPasswordBox),
            new PropertyMetadata(null, new PropertyChangedCallback(OnRichPasswordBoxMinimumCharCountDisplayTextChanged)));

        public bool IsValidPassword
        {
            get { return (bool)GetValue(IsValidPasswordProperty); }
            set { SetValue(IsValidPasswordProperty, value); }
        }

        public static readonly DependencyProperty IsValidPasswordProperty = DependencyProperty.Register("IsValidPassword",
            typeof(bool),
            typeof(RichPasswordBox),
            new PropertyMetadata(null, new PropertyChangedCallback(OnRichPasswordBoxIsValidPasswordChanged)));

        public bool HasValidLowercase { get; private set; }
        public bool HasValidUppercase { get; private set; }
        public bool HasValidNumber { get; private set; }
        public bool HasValidMinimumChars { get; private set; }

        public RichPasswordBox()
        {
            this.InitializeComponent();

            CurrentInstance = this;

            DataContext = this;
            
            InitialHideValidationStackPanel.Begin();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowValidationStackPanel.Begin();
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HideValidationStackPanel.Begin();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;

            if (!string.IsNullOrEmpty(passwordBox.Password))
            {
                HasValidLowercase = passwordBox.Password.Any(c => char.IsLower(c));
                HasValidUppercase = passwordBox.Password.Any(c => char.IsUpper(c));
                HasValidNumber = passwordBox.Password.Any(c => char.IsNumber(c));
                HasValidMinimumChars = (passwordBox.Password.Length >= MinimumCharCount);

                lblLowercase.Foreground = (HasValidLowercase) ? GreenColor : RedColor;
                lblUppercase.Foreground = (HasValidUppercase) ? GreenColor : RedColor;
                lblNumber.Foreground = (HasValidNumber) ? GreenColor : RedColor;
                lblMinChars.Foreground = (HasValidMinimumChars) ? GreenColor : RedColor;

                IsValidPassword = (HasValidLowercase && HasValidUppercase && HasValidNumber && HasValidMinimumChars);
            }
            else
            {
                lblLowercase.Foreground = lblUppercase.Foreground = lblNumber.Foreground = lblMinChars.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private static void OnRichPasswordBoxMinimumCharCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnRichPasswordBoxMinimumCharCountDisplayTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CurrentInstance.lblMinChars.Text = CurrentInstance.MinimumCharCountDisplayText;
        }

        private static void OnRichPasswordBoxIsValidPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
