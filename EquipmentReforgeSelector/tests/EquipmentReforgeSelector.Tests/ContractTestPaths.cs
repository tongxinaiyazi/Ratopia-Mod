using System;
using System.IO;

namespace EquipmentReforgeSelector.Tests
{
    internal static class ContractTestPaths
    {
        public static string RepositoryRoot
        {
            get
            {
                var directory = new DirectoryInfo(AppContext.BaseDirectory);
                while (directory != null && !File.Exists(Path.Combine(directory.FullName, "EquipmentReforgeSelector.sln")))
                {
                    directory = directory.Parent;
                }

                if (directory == null)
                {
                    throw new DirectoryNotFoundException("Could not locate the repository root.");
                }

                return directory.FullName;
            }
        }

        public static string GameDirectory =>
            Environment.GetEnvironmentVariable("RATOPIA_DIR") ?? @"E:\steam\steamapps\common\Ratopia";

        public static string ProductionFile(string name) =>
            Path.Combine(RepositoryRoot, "src", "EquipmentReforgeSelector", name);
    }
}
