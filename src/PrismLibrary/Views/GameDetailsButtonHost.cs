using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Playnite.SDK.Models;

namespace PrismLibrary.Views
{
    /// <summary>
    /// Injects pack-update actions as siblings of Playnite's native Play/More buttons
    /// so they pick up the same size and theme styling.
    /// </summary>
    internal sealed class GameDetailsButtonHost
    {
        private const string CheckTag = "PrismLibrary.CheckUpdateButton";
        private const string UpdateTag = "PrismLibrary.UpdateInPrismButton";
        private const string StatusTag = "PrismLibrary.UpdateStatusText";

        private readonly PrismLibraryPlugin plugin;
        private Button checkButton;
        private Button updateButton;
        private TextBlock statusText;

        public GameDetailsButtonHost(PrismLibraryPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void Sync(Game selectedGame)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => AttachOrUpdate(selectedGame));
        }

        private void AttachOrUpdate(Game selectedGame)
        {
            try
            {
                var window = Application.Current?.MainWindow;
                if (window == null)
                {
                    return;
                }

                // Theme already hosts the plugin control — don't inject duplicates.
                if (FindDescendantByName(window, "PrismLibrary_PackUpdateButtons") != null)
                {
                    Detach();
                    return;
                }

                // Remove the previous nested PluginUserControl injection if still present.
                var legacy = FindDescendantByTag(window, PackUpdateButtonsView.HostTag) as FrameworkElement;
                if (legacy?.Parent is Panel legacyParent)
                {
                    legacyParent.Children.Remove(legacy);
                }

                var moreButton = FindDescendantByName(window, "PART_ButtonMoreActions") as Button;
                var actionRow = moreButton?.Parent as Panel;
                if (moreButton == null || actionRow == null)
                {
                    Detach();
                    return;
                }

                EnsureButtons(actionRow, moreButton);
                EnsureStatusText(actionRow);

                var state = plugin.GetPackUpdateUiState(selectedGame);
                ApplyState(state);
            }
            catch
            {
                // Theme layouts vary; never break Playnite UI if attachment fails.
            }
        }

        private void EnsureButtons(Panel actionRow, Button moreButton)
        {
            checkButton = FindInPanelByTag(actionRow, CheckTag) as Button;
            updateButton = FindInPanelByTag(actionRow, UpdateTag) as Button;

            if (checkButton == null)
            {
                checkButton = CreateActionButton("Check for update", CheckTag, moreButton);
                checkButton.Click += (_, __) => plugin.CheckSelectedPackUpdateFromUi();
                InsertBefore(actionRow, moreButton, checkButton);
            }

            if (updateButton == null)
            {
                updateButton = CreateActionButton("Update in Prism", UpdateTag, moreButton);
                updateButton.Click += (_, __) => plugin.OpenSelectedUpdateInPrismFromUi();
                InsertBefore(actionRow, moreButton, updateButton);
            }
            else
            {
                // Keep label short even if an older build set a long Content.
                updateButton.Content = "Update in Prism";
            }
        }

        private void EnsureStatusText(Panel actionRow)
        {
            var titleStack = actionRow.Parent as Panel;
            if (titleStack == null)
            {
                return;
            }

            statusText = FindInPanelByTag(titleStack, StatusTag) as TextBlock;
            if (statusText != null)
            {
                return;
            }

            statusText = new TextBlock
            {
                Tag = StatusTag,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed
            };
            statusText.SetResourceReference(TextBlock.ForegroundProperty, "TextBrush");

            var rowIndex = titleStack.Children.IndexOf(actionRow);
            if (rowIndex >= 0 && rowIndex + 1 <= titleStack.Children.Count)
            {
                titleStack.Children.Insert(rowIndex + 1, statusText);
            }
            else
            {
                titleStack.Children.Add(statusText);
            }
        }

        private void ApplyState(PackUpdateUiState state)
        {
            var showCheck = state?.IsVisible == true && state.CanCheck;
            var showUpdate = state?.IsVisible == true && state.CanUpdate;

            if (checkButton != null)
            {
                checkButton.Visibility = showCheck ? Visibility.Visible : Visibility.Collapsed;
                checkButton.IsEnabled = showCheck;
            }

            if (updateButton != null)
            {
                updateButton.Visibility = showUpdate ? Visibility.Visible : Visibility.Collapsed;
                updateButton.IsEnabled = showUpdate;
                updateButton.Content = "Update in Prism";
                updateButton.ToolTip = showUpdate && !string.IsNullOrWhiteSpace(state?.StatusText)
                    ? state.StatusText
                    : null;
            }

            if (statusText != null)
            {
                if (state?.IsVisible == true && !string.IsNullOrWhiteSpace(state.StatusText))
                {
                    statusText.Text = state.StatusText;
                    statusText.Visibility = Visibility.Visible;
                }
                else
                {
                    statusText.Text = string.Empty;
                    statusText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static Button CreateActionButton(string content, string tag, Button moreButton)
        {
            var button = new Button
            {
                Content = content,
                Tag = tag,
                Width = moreButton.Width > 0 ? moreButton.Width : 150,
                Height = moreButton.Height > 0 ? moreButton.Height : 40,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = moreButton.VerticalAlignment,
                HorizontalAlignment = moreButton.HorizontalAlignment,
                Focusable = false
            };

            if (moreButton.Style != null)
            {
                button.Style = moreButton.Style;
            }

            button.FontSize = moreButton.FontSize;
            button.FontFamily = moreButton.FontFamily;
            button.Padding = moreButton.Padding;
            return button;
        }

        private static void InsertBefore(Panel panel, UIElement before, UIElement element)
        {
            var index = panel.Children.IndexOf(before);
            if (index >= 0)
            {
                panel.Children.Insert(index, element);
            }
            else
            {
                panel.Children.Add(element);
            }
        }

        private void Detach()
        {
            RemoveFromParent(checkButton);
            RemoveFromParent(updateButton);
            RemoveFromParent(statusText);
            checkButton = null;
            updateButton = null;
            statusText = null;
        }

        private static void RemoveFromParent(FrameworkElement element)
        {
            if (element?.Parent is Panel panel)
            {
                panel.Children.Remove(element);
            }
        }

        private static FrameworkElement FindInPanelByTag(Panel panel, object tag)
        {
            if (panel == null)
            {
                return null;
            }

            foreach (var child in panel.Children)
            {
                if (child is FrameworkElement fe && Equals(fe.Tag, tag))
                {
                    return fe;
                }
            }

            return null;
        }

        private static DependencyObject FindDescendantByName(DependencyObject root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (root is FrameworkElement fe && fe.Name == name)
            {
                return root;
            }

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var found = FindDescendantByName(VisualTreeHelper.GetChild(root, i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static DependencyObject FindDescendantByTag(DependencyObject root, object tag)
        {
            if (root == null || tag == null)
            {
                return null;
            }

            if (root is FrameworkElement fe && Equals(fe.Tag, tag))
            {
                return root;
            }

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var found = FindDescendantByTag(VisualTreeHelper.GetChild(root, i), tag);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
