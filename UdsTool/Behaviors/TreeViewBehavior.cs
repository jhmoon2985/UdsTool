// Behaviors/TreeViewBehavior.cs
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace UdsTool.Behaviors
{
    public class TreeViewBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as TreeViewBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                behavior.UpdateSelectedItem();
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }

        private void UpdateSelectedItem()
        {
            if (AssociatedObject == null) return;

            if (SelectedItem != null)
            {
                SelectItem(AssociatedObject, SelectedItem);
            }
            else
            {
                ClearTreeViewSelection(AssociatedObject);
            }
        }

        private void SelectItem(ItemsControl itemsControl, object item)
        {
            var treeView = itemsControl as TreeView;
            if (treeView != null && treeView.SelectedItem == item) return;

            // 트리뷰 항목이 생성되었는지 확인
            if (itemsControl.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                // 컨테이너가 생성되지 않았다면 이후에 다시 시도
                EventHandler handler = null;
                handler = (sender, args) =>
                {
                    if (itemsControl.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        itemsControl.ItemContainerGenerator.StatusChanged -= handler;
                        SelectItem(itemsControl, item);
                    }
                };
                itemsControl.ItemContainerGenerator.StatusChanged += handler;
                return;
            }

            // 선택할 항목을 현재 레벨에서 검색
            var itemContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (itemContainer != null)
            {
                itemContainer.IsSelected = true;
                itemContainer.BringIntoView();
                return;
            }

            // 현재 레벨에서 찾을 수 없으면, 자식 항목들에서 검색
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                var childItem = itemsControl.Items[i];
                var childContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                if (childContainer != null)
                {
                    childContainer.IsExpanded = true;
                    SelectItem(childContainer, item);
                }
            }
        }

        private void ClearTreeViewSelection(TreeView treeView)
        {
            // TreeView.SelectedItem을 null로 설정하는 방법은 쉽지 않으므로
            // 비공개 ClearValue 메서드를 리플렉션으로 호출
            var selectedItemProperty = treeView.GetType().GetProperty("SelectedItem");
            if (selectedItemProperty != null)
            {
                var clearMethod = selectedItemProperty.GetType().GetMethod("ClearValue", BindingFlags.Instance | BindingFlags.NonPublic);
                if (clearMethod != null)
                {
                    clearMethod.Invoke(selectedItemProperty, new object[] { });
                }
            }
        }
    }
}