using System;

namespace ExecutionPlatform.Runtime
{
    internal static class ExecutionVisuals
    {
        private const string ExecutionBuildingSpritePath =
            "GameScene/Map/Building/Building_10001";
        private const string PrisonBuildingSpritePath =
            "GameScene/Map/Building/Building_Prison";

        internal static string ResolveSpritePath(string path)
        {
            return string.Equals(path, ExecutionBuildingSpritePath, StringComparison.Ordinal)
                ? PrisonBuildingSpritePath
                : path;
        }

        internal static bool RequiresOrdinaryFrame(BuildInfo info)
        {
            return ExecutionCatalog.IsExecution(info);
        }
    }
}
