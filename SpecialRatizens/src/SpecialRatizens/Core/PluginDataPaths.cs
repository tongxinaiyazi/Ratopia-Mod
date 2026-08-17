using System;
using System.IO;

namespace SpecialRatizens.Core
{
    internal static class PluginDataPaths
    {
        public static string ResolveDataRoot(string assemblyLocation)
        {
            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                throw new ArgumentException("插件程序集路径不能为空。", nameof(assemblyLocation));
            }

            var assemblyDirectory = Path.GetDirectoryName(Path.GetFullPath(assemblyLocation));
            return Path.Combine(assemblyDirectory, "Data");
        }
    }
}
