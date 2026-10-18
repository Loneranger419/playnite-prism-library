using System;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace PrismLibrary.Controllers
{
    public sealed class PrismInstallController : InstallController
    {
        private readonly Action<string> enableInstance;
        private readonly Func<string, string> resolveInstallDirectory;

        public PrismInstallController(
            Game game,
            Action<string> enableInstance,
            Func<string, string> resolveInstallDirectory) : base(game)
        {
            Name = "Enable Prism instance";
            this.enableInstance = enableInstance;
            this.resolveInstallDirectory = resolveInstallDirectory;
        }

        public override void Install(InstallActionArgs args)
        {
            enableInstance?.Invoke(Game.GameId);
            var installDirectory = resolveInstallDirectory?.Invoke(Game.GameId) ?? Game.InstallDirectory;
            InvokeOnInstalled(new GameInstalledEventArgs(new GameInstallationData
            {
                InstallDirectory = installDirectory
            }));
        }
    }
}
