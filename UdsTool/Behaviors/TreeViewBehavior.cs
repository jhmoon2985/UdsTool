using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace UdsTool.Behaviors
{
    public class TreeViewItemBehavior : Behavior<TreeViewItem>
    {
        public static readonly DependencyProperty SelectedItemChangedCommandProperty =
            DependencyProperty.Register(
                "SelectedItemChangedCommand",
                typeof(ICommand),
                typeof(TreeViewItemBehavior),
                new PropertyMetadata(null));

        public ICommand SelectedItemChangedCommand
        {
            get => (ICommand)GetValue(SelectedItemChangedCommandProperty);
            set => SetValue(SelectedItemChangedCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Selected += OnTreeViewItemSelected;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Selected -= OnTreeViewItemSelected;
            base.OnDetaching();
        }

        private void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            if (SelectedItemChangedCommand != null &&
                SelectedItemChangedCommand.CanExecute(AssociatedObject.DataContext))
            {
                SelectedItemChangedCommand.Execute(AssociatedObject.DataContext);
            }
        }
    }

    public class TreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(TreeViewSelectedItemBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            base.OnDetaching();
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }
    }
}