using System.IO;
using PrismLibrary.Services;
using Xunit;

namespace PrismLibrary.Tests
{
    public class PrismPathsTests
    {
        [Fact]
        public void ResolveInstancesDirectory_UsesInstanceDirFromCfg()
        {
            var root = Path.Combine(Path.GetTempPath(), "PrismLibraryTests_" + Path.GetRandomFileName());
            var customInstances = Path.Combine(root, "MyInstances");
            Directory.CreateDirectory(customInstances);
            File.WriteAllText(Path.Combine(root, "prismlauncher.cfg"), "InstanceDir=" + customInstances);

            try
            {
                var resolved = PrismPaths.ResolveInstancesDirectory(root);
                Assert.Equal(Path.GetFullPath(customInstances), resolved);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [Fact]
        public void NormalizeExecutablePath_AcceptsDirectoryContainingExe()
        {
            var root = Path.Combine(Path.GetTempPath(), "PrismLibraryTests_" + Path.GetRandomFileName());
            Directory.CreateDirectory(root);
            var exe = Path.Combine(root, "prismlauncher.exe");
            File.WriteAllBytes(exe, new byte[] { 0 });

            try
            {
                var normalized = PrismPaths.NormalizeExecutablePath(root);
                Assert.Equal(Path.GetFullPath(exe), normalized);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}
