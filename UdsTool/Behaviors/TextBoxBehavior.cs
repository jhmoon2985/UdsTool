using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace UdsTool.Behaviors
{
    public class TextBoxBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty AcceptHexInputProperty =
            DependencyProperty.Register(
                nameof(AcceptHexInput),
                typeof(bool),
                typeof(TextBoxBehavior),
                new PropertyMetadata(false));

        public bool AcceptHexInput
        {
            get => (bool)GetValue(AcceptHexInputProperty);
            set => SetValue(AcceptHexInputProperty, value);
        }

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
            if (AcceptHexInput)
            {
                // 16진수 입력만 허용
                e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9A-Fa-f]+$");
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 붙여넣기 허용
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = false;
            }
            // 복사 허용
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = false;
            }
        }
    }
}