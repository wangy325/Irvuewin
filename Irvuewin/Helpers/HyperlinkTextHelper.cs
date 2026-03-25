using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Irvuewin.Helpers
{
    public static class HyperlinkTextHelper
    {
        public static readonly DependencyProperty FormatTextProperty =
            DependencyProperty.RegisterAttached("FormatText", typeof(string), typeof(HyperlinkTextHelper), new PropertyMetadata(null, OnFormatTextChanged));

        public static readonly DependencyProperty LinkTextProperty =
            DependencyProperty.RegisterAttached("LinkText", typeof(string), typeof(HyperlinkTextHelper), new PropertyMetadata(null, OnFormatTextChanged));

        public static readonly DependencyProperty LinkCommandProperty =
            DependencyProperty.RegisterAttached("LinkCommand", typeof(ICommand), typeof(HyperlinkTextHelper), new PropertyMetadata(null, OnFormatTextChanged));

        public static readonly DependencyProperty LinkCommandParameterProperty =
            DependencyProperty.RegisterAttached("LinkCommandParameter", typeof(object), typeof(HyperlinkTextHelper), new PropertyMetadata(null, OnFormatTextChanged));

        public static readonly DependencyProperty LinkForegroundProperty =
            DependencyProperty.RegisterAttached("LinkForeground", typeof(Brush), typeof(HyperlinkTextHelper), new PropertyMetadata(null, OnFormatTextChanged));

        public static string GetFormatText(DependencyObject obj) => (string)obj.GetValue(FormatTextProperty);
        public static void SetFormatText(DependencyObject obj, string value) => obj.SetValue(FormatTextProperty, value);

        public static string GetLinkText(DependencyObject obj) => (string)obj.GetValue(LinkTextProperty);
        public static void SetLinkText(DependencyObject obj, string value) => obj.SetValue(LinkTextProperty, value);

        public static ICommand GetLinkCommand(DependencyObject obj) => (ICommand)obj.GetValue(LinkCommandProperty);
        public static void SetLinkCommand(DependencyObject obj, ICommand value) => obj.SetValue(LinkCommandProperty, value);

        public static object GetLinkCommandParameter(DependencyObject obj) => obj.GetValue(LinkCommandParameterProperty);
        public static void SetLinkCommandParameter(DependencyObject obj, object value) => obj.SetValue(LinkCommandParameterProperty, value);

        public static Brush GetLinkForeground(DependencyObject obj) => (Brush)obj.GetValue(LinkForegroundProperty);
        public static void SetLinkForeground(DependencyObject obj, Brush value) => obj.SetValue(LinkForegroundProperty, value);

        private static void OnFormatTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock) return;
            var format = GetFormatText(textBlock);
            var linkText = GetLinkText(textBlock);
            var command = GetLinkCommand(textBlock);
            var param = GetLinkCommandParameter(textBlock);
            var foreground = GetLinkForeground(textBlock);

            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(format)) return;

            var parts = format.Split(["{0}"], StringSplitOptions.None);
                
            if (parts.Length > 0)
            {
                textBlock.Inlines.Add(new Run(parts[0]));
            }

            if (parts.Length <= 1) return;
            var linkRun = new Run(linkText);
            var hyperlink = new Hyperlink(linkRun)
            {
                Command = command,
                CommandParameter = param,
                Foreground = foreground
            };

            textBlock.Inlines.Add(hyperlink);
            textBlock.Inlines.Add(new Run(parts[1]));
        }
    }
}
