using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using UdsTool.Models;
using UdsTool.ViewModels;

namespace UdsTool.Behaviors
{
    public class TreeViewBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(TreeViewBehavior),
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
            AssociatedObject.MouseDoubleClick += OnTreeViewMouseDoubleClick;
            AssociatedObject.KeyDown += OnTreeViewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            AssociatedObject.MouseDoubleClick -= OnTreeViewMouseDoubleClick;
            AssociatedObject.KeyDown -= OnTreeViewKeyDown;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;

            if (AssociatedObject.DataContext is XmlEditorViewModel viewModel)
            {
                viewModel.SelectedFrame = e.NewValue as DiagnosticFrame;
            }
        }

        private void OnTreeViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.SelectedItem is DiagnosticFrame frame)
            {
                if (AssociatedObject.DataContext is XmlEditorViewModel viewModel)
                {
                    viewModel.EditFrameCommand.Execute(frame);
                }
            }
        }

        private void OnTreeViewKeyDown(object sender, KeyEventArgs e)
        {
            if (AssociatedObject.DataContext is XmlEditorViewModel viewModel)
            {
                if (e.Key == Key.Delete)
                {
                    if (AssociatedObject.SelectedItem is DiagnosticFrame frame)
                    {
                        viewModel.DeleteFrameCommand.Execute(frame);
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.F2)
                {
                    if (AssociatedObject.SelectedItem is DiagnosticFrame frame)
                    {
                        viewModel.EditFrameCommand.Execute(frame);
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Insert)
                {
                    if (AssociatedObject.SelectedItem is DiagnosticFrame frame)
                    {
                        if (frame.Type == RequestResponseType.Request)
                        {
                            viewModel.AddResponseToSelectedCommand.Execute(frame);
                        }
                        else
                        {
                            viewModel.AddRequestCommand.Execute(null);
                        }
                        e.Handled = true;
                    }
                    else
                    {
                        viewModel.AddRequestCommand.Execute(null);
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Up && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    viewModel.MoveUpCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    viewModel.MoveDownCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}