using System;
using System.IO;

namespace PrismLibrary.Services
{
    public sealed class PrismPathInfo
    {
        public string ExecutablePath { get; set; }
        public string DataRoot { get; set; }
        public string InstancesDirectory { get; set; }
        public string IconsDirectory { get; set; }
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(ExecutablePath) &&
            File.Exists(ExecutablePath) &&
            !string.IsNullOrWhiteSpace(DataRoot) &&
            Directory.Exists(DataRoot) &&
            !string.IsNullOrWhiteSpace(InstancesDirectory) &&
            Directory.Exists(InstancesDirectory);
    }

    public static class PrismPaths
    {
        public const string ExecutableFileName = "prismlauncher.exe";

        public static PrismPathInfo Resolve(string executablePath, string dataRoot)
        {
            var exe = NormalizeExecutablePath(executablePath);
            var root = string.IsNullOrWhiteSpace(dataRoot)
                ? null
                : Path.GetFullPath(dataRoot.Trim().Trim('"'));

            string instancesDir = null;
            string iconsDir = null;

            if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
            {
                instancesDir = ResolveInstancesDirectory(root);
                iconsDir = Path.Combine(root, "icons");
            }

            return new PrismPathInfo
            {
                ExecutablePath = exe,
                DataRoot = root,
                InstancesDirectory = instancesDir,
                IconsDirectory = iconsDir
            };
        }

        public static PrismPathInfo DetectDefaults()
        {
            var localPrograms = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs",
                "PrismLauncher",
                ExecutableFileName);

            var roamingData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PrismLauncher");

            string exe = null;
            if (File.Exists(localPrograms))
            {
                exe = localPrograms;
            }
            else
            {
                var portableExe = Path.Combine(roamingData, ExecutableFileName);
                if (File.Exists(portableExe))
                {
                    exe = portableExe;
                }
            }

            string dataRoot = null;
            if (Directory.Exists(roamingData))
            {
                dataRoot = roamingData;
            }
            else if (!string.IsNullOrWhiteSpace(exe))
            {
                var exeDir = Path.GetDirectoryName(exe);
                if (!string.IsNullOrWhiteSpace(exeDir) && Directory.Exists(Path.Combine(exeDir, "instances")))
                {
                    dataRoot = exeDir;
                }
            }

            return Resolve(exe, dataRoot);
        }

        public static string ResolveInstancesDirectory(string dataRoot)
        {
            var cfgPath = Path.Combine(dataRoot, "prismlauncher.cfg");
            if (File.Exists(cfgPath))
            {
                var values = IniKeyValueParser.ParseFile(cfgPath);
                if (values.TryGetValue("InstanceDir", out var instanceDir) && !string.IsNullOrWhiteSpace(instanceDir))
                {
                    var resolved = instanceDir.Trim().Trim('"');
                    if (!Path.IsPathRooted(resolved))
                    {
                        resolved = Path.Combine(dataRoot, resolved);
                    }

                    return Path.GetFullPath(resolved);
                }
            }

            return Path.Combine(dataRoot, "instances");
        }

        public static string NormalizeExecutablePath(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                return null;
            }

            var path = executablePath.Trim().Trim('"');
            if (Directory.Exists(path))
            {
                var candidate = Path.Combine(path, ExecutableFileName);
                return File.Exists(candidate) ? Path.GetFullPath(candidate) : Path.GetFullPath(path);
            }

            return File.Exists(path) ? Path.GetFullPath(path) : path;
        }
    }
}
