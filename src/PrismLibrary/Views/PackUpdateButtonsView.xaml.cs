using System.Windows;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;

namespace PrismLibrary.Views
{
    public partial class PackUpdateButtonsView : PluginUserControl
    {
        public const string HostTag = "PrismLibrary.PackUpdateButtons";

        private readonly PrismLibraryPlugin plugin;

        public PackUpdateButtonsView(PrismLibraryPlugin plugin)
        {
            this.plugin = plugin;
            Tag = HostTag;
            InitializeComponent();
            Refresh();
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (plugin == null || PanelRoot == null)
            {
                return;
            }

            var state = plugin.GetPackUpdateUiState(GameContext);
            PanelRoot.Visibility = state.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            if (!state.IsVisible)
            {
                return;
            }

            ButtonCheck.Visibility = state.CanCheck ? Visibility.Visible : Visibility.Collapsed;
            ButtonCheck.IsEnabled = state.CanCheck;
            ButtonUpdate.Visibility = state.CanUpdate ? Visibility.Visible : Visibility.Collapsed;
            ButtonUpdate.IsEnabled = state.CanUpdate;
            ButtonUpdate.Content = "Update in Prism";
            ButtonUpdate.ToolTip = state.CanUpdate && !string.IsNullOrWhiteSpace(state.StatusText)
                ? state.StatusText
                : null;
            StatusText.Text = state.StatusText ?? string.Empty;
            StatusText.Visibility = string.IsNullOrWhiteSpace(state.StatusText)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            if (GameContext == null)
            {
                return;
            }

            plugin.CheckPackUpdateFromUi(GameContext.GameId);
            Refresh();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (GameContext == null)
            {
                return;
            }

            plugin.OpenUpdateInPrismFromUi(GameContext.GameId);
        }
    }
}
