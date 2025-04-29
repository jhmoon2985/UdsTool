using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using UdsTool.Models;
using UdsTool.ViewModels;

namespace UdsTool.Behaviors
{
    public class TreeViewBehavior : Behavior<FrameworkElement>
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

            if (AssociatedObject is TreeView treeView)
            {
                treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
                treeView.MouseDoubleClick += OnTreeViewMouseDoubleClick;
                treeView.KeyDown += OnTreeViewKeyDown;
            }
            else if (AssociatedObject is ListView listView)
            {
                listView.SelectionChanged += OnListViewSelectionChanged;
                listView.MouseDoubleClick += OnListViewMouseDoubleClick;
                listView.KeyDown += OnListViewKeyDown;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is TreeView treeView)
            {
                treeView.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
                treeView.MouseDoubleClick -= OnTreeViewMouseDoubleClick;
                treeView.KeyDown -= OnTreeViewKeyDown;
            }
            else if (AssociatedObject is ListView listView)
            {
                listView.SelectionChanged -= OnListViewSelectionChanged;
                listView.MouseDoubleClick -= OnListViewMouseDoubleClick;
                listView.KeyDown -= OnListViewKeyDown;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;

            if (AssociatedObject.DataContext is XmlEditorViewModel viewModel)
            {
                viewModel.SelectedFrame = e.NewValue as DiagnosticFrame;
            }
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem != null)
            {
                SelectedItem = listView.SelectedItem;

                if (AssociatedObject.DataContext is XmlEditorViewModel viewModel)
                {
                    var selectedFrame = listView.SelectedItem as DiagnosticFrame;
                    viewModel.SelectedFrame = selectedFrame;

                    // Request 또는 Response 리스트에 따라 적절한 SelectedFrame 설정
                    if (selectedFrame.Type == RequestResponseType.Request)
                    {
                        viewModel.SelectedRequestFrame = selectedFrame;
                    }
                    else
                    {
                        viewModel.SelectedResponseFrame = selectedFrame;
                    }
                }
            }
        }

        private void OnTreeViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.DataContext is XmlEditorViewModel viewModel &&
                ((TreeView)sender).SelectedItem is DiagnosticFrame frame)
            {
                viewModel.EditFrameCommand.Execute(frame);
            }
        }

        private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.DataContext is XmlEditorViewModel viewModel &&
                ((ListView)sender).SelectedItem is DiagnosticFrame frame)
            {
                viewModel.EditFrameCommand.Execute(frame);
            }
        }

        private void OnTreeViewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(sender as TreeView, e);
        }

        private void OnListViewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(sender as ListView, e);
        }

        private void HandleKeyDown(FrameworkElement sender, KeyEventArgs e)
        {
            if (AssociatedObject.DataContext is XmlEditorViewModel viewModel)
            {
                DiagnosticFrame frame = null;

                if (sender is TreeView treeView)
                {
                    frame = treeView.SelectedItem as DiagnosticFrame;
                }
                else if (sender is ListView listView)
                {
                    frame = listView.SelectedItem as DiagnosticFrame;
                }

                if (frame != null)
                {
                    switch (e.Key)
                    {
                        case Key.Delete:
                            viewModel.DeleteFrameCommand.Execute(frame);
                            e.Handled = true;
                            break;
                        case Key.F2:
                            viewModel.EditFrameCommand.Execute(frame);
                            e.Handled = true;
                            break;
                        case Key.Insert:
                            if (frame.Type == RequestResponseType.Request)
                            {
                                viewModel.AddResponseToSelectedCommand.Execute(frame);
                            }
                            else
                            {
                                if (frame.Type == RequestResponseType.Request)
                                    viewModel.AddResponseCommand.Execute(null);
                                else
                                    viewModel.AddRequestCommand.Execute(null);
                            }
                            e.Handled = true;
                            break;
                        case Key.Up when (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control:
                            viewModel.MoveUpCommand.Execute(null);
                            e.Handled = true;
                            break;
                        case Key.Down when (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control:
                            viewModel.MoveDownCommand.Execute(null);
                            e.Handled = true;
                            break;
                    }
                }
                else if (e.Key == Key.Insert)
                {
                    // 선택된 항목이 없을 때는 현재 활성화된 목록에 따라 Request 또는 Response 추가
                    if (sender.Name.Contains("Request"))
                        viewModel.AddRequestCommand.Execute(null);
                    else
                        viewModel.AddResponseCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}