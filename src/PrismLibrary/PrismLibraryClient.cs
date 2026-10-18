using System.Diagnostics;
using System.IO;
using Playnite.SDK;
using PrismLibrary.Services;

namespace PrismLibrary
{
    public class PrismLibraryClient : LibraryClient
    {
        private readonly PrismLibraryPlugin plugin;

        public PrismLibraryClient(PrismLibraryPlugin plugin)
        {
            this.plugin = plugin;
        }

        public override bool IsInstalled
        {
            get
            {
                var paths = plugin.GetResolvedPaths();
                return paths != null && File.Exists(paths.ExecutablePath);
            }
        }

        public override void Open()
        {
            var paths = plugin.GetResolvedPaths();
            if (paths == null || string.IsNullOrWhiteSpace(paths.ExecutablePath) || !File.Exists(paths.ExecutablePath))
            {
                throw new System.InvalidOperationException("Prism Launcher executable is not configured.");
            }

            var args = string.IsNullOrWhiteSpace(paths.DataRoot)
                ? string.Empty
                : "--dir " + LaunchBuilder.Quote(paths.DataRoot);

            Process.Start(new ProcessStartInfo
            {
                FileName = paths.ExecutablePath,
                Arguments = args,
                UseShellExecute = true
            });
        }
    }
}
