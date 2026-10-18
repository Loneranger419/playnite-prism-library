using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Models;

namespace PrismLibrary.Services
{
    public static class LaunchBuilder
    {
        public static string BuildLaunchArguments(string dataRoot, string instanceId, string profile)
        {
            var builder = new StringBuilder();
            AppendArg(builder, "--dir", dataRoot);
            AppendArg(builder, "--launch", instanceId);
            if (!string.IsNullOrWhiteSpace(profile))
            {
                AppendArg(builder, "--profile", profile);
            }

            return builder.ToString().Trim();
        }

        public static string BuildShowArguments(string dataRoot, string instanceId)
        {
            var builder = new StringBuilder();
            AppendArg(builder, "--dir", dataRoot);
            AppendArg(builder, "--show", instanceId);
            return builder.ToString().Trim();
        }

        public static List<GameAction> BuildActions(
            string executablePath,
            string dataRoot,
            string instanceId,
            string instanceDirectory,
            string minecraftDirectory,
            string profile,
            bool isEnabled)
        {
            var actions = new List<GameAction>();

            if (isEnabled)
            {
                actions.Add(new GameAction
                {
                    Name = "Play",
                    Type = GameActionType.File,
                    Path = executablePath,
                    Arguments = BuildLaunchArguments(dataRoot, instanceId, profile),
                    IsPlayAction = true
                });
            }

            actions.Add(new GameAction
            {
                Name = "Open in Prism",
                Type = GameActionType.File,
                Path = executablePath,
                Arguments = BuildShowArguments(dataRoot, instanceId),
                IsPlayAction = false
            });

            if (!string.IsNullOrWhiteSpace(instanceDirectory))
            {
                actions.Add(new GameAction
                {
                    Name = "Open Instance Folder",
                    Type = GameActionType.File,
                    Path = "explorer.exe",
                    Arguments = Quote(instanceDirectory),
                    IsPlayAction = false
                });
            }

            if (!string.IsNullOrWhiteSpace(minecraftDirectory))
            {
                actions.Add(new GameAction
                {
                    Name = "Open .minecraft Folder",
                    Type = GameActionType.File,
                    Path = "explorer.exe",
                    Arguments = Quote(minecraftDirectory),
                    IsPlayAction = false
                });
            }

            return actions;
        }

        private static void AppendArg(StringBuilder builder, string name, string value)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(name);
            builder.Append(' ');
            builder.Append(Quote(value));
        }

        public static string Quote(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }

            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }
}
