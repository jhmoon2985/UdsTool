using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace UdsTool.Behaviors
{
    public class HexTextBoxBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 16진수와 공백만 허용
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9A-Fa-f ]+$");
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+V, Ctrl+C 허용
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.V || e.Key == Key.C)
                {
                    e.Handled = false;
                }
            }
        }
    }
}