using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace UdsTool.Behaviors
{
    public class ScrollBehavior : Behavior<ScrollViewer>
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.Register(
                nameof(AutoScroll),
                typeof(bool),
                typeof(ScrollBehavior),
                new PropertyMetadata(false));

        public bool AutoScroll
        {
            get => (bool)GetValue(AutoScrollProperty);
            set => SetValue(AutoScrollProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.ScrollChanged -= ScrollViewer_ScrollChanged;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (AutoScroll && e.ExtentHeightChange > 0)
            {
                AssociatedObject.ScrollToBottom();
            }
        }
    }
}