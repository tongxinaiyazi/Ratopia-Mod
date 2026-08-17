using BepInEx;
using HarmonyLib;
using ScaffoldMod.Runtime;

namespace ScaffoldMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "cn.ratopia.scaffold";
        internal const string PluginName = "脚手架";
        internal const string PluginVersion = "0.1.0";

        private Harmony harmony;

        private void Awake()
        {
            ScaffoldRuntime.Initialize(Logger);
            harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo("脚手架 0.1.0 已加载，Harmony 补丁安装完成。");
        }

        private void OnDestroy()
        {
            ScaffoldRuntime.Shutdown();
            harmony?.UnpatchSelf();
        }
    }
}
