using System;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace PrismLibrary.Controllers
{
    public sealed class PrismUninstallController : UninstallController
    {
        private readonly Action<string> disableInstance;

        public PrismUninstallController(Game game, Action<string> disableInstance) : base(game)
        {
            Name = "Disable Prism instance (keeps Prism files)";
            this.disableInstance = disableInstance;
        }

        public override void Uninstall(UninstallActionArgs args)
        {
            disableInstance?.Invoke(Game.GameId);
            InvokeOnUninstalled(new GameUninstalledEventArgs());
        }
    }
}
