using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using SuperBow.Patches;
using SuperBow.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuperBow
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class SuperBowPlugin : BaseUnityPlugin
    {
        public const string PluginName = "超级弓箭";
        public const string PluginGuid = "cn.ratopia.superbow";
        public const string PluginVersion = "0.1.2";

        private Harmony _harmony;
        private bool _sceneHooked;

        private void Awake()
        {
            gameObject.hideFlags |= HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            Logger.LogInfo($"发现插件：{PluginName} {PluginVersion}");

            RuntimeCatalog.Initialize(Logger);
            CombatRuntime.Initialize(Logger);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            _sceneHooked = true;

            _harmony = new Harmony(PluginGuid);
            var patchTypes = new[]
            {
                typeof(ItemDatabasePatch),
                typeof(ItemDetailReforgeContextPatch),
                typeof(ItemEnhanceDatabasePatch),
                typeof(ItemEnhanceReforgeContextPatch),
                typeof(BowArrowHitPatch),
                typeof(DamageDisplayPatch),
                typeof(TooltipPatch),
                typeof(Tooltip2Patch),
                typeof(RuntimeTickPatch)
            };

            try
            {
                foreach (var patchType in patchTypes.OrderBy(type => type.FullName, StringComparer.Ordinal))
                {
                    Logger.LogInfo($"安装补丁：{patchType.FullName}");
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.LogInfo($"补丁已安装：{patchType.FullName}");
                }

                var manager = GameMgr.Instance;
                RuntimeCatalog.TryApplySafely(manager != null ? manager._DB_Mgr : null);
            }
            catch (Exception exception)
            {
                Logger.LogError($"补丁安装失败，已回滚并停用超级弓箭：{exception}");
                _harmony.UnpatchSelf();
                RuntimeCatalog.Shutdown();
                CombatRuntime.Disable("Harmony 补丁未能完整安装");
            }
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            RuntimeCatalog.ClearReforgeContext();
            CombatRuntime.Clear(
                $"场景从 {previous.name} 切换到 {next.name}，已清理流血状态。");
        }

        private void OnDestroy()
        {
            Logger.LogWarning("超级弓箭插件对象正在销毁，清理运行时状态与目录修改。");
            if (_sceneHooked)
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
                _sceneHooked = false;
            }

            CombatRuntime.Shutdown();
            RuntimeCatalog.Shutdown();
            _harmony?.UnpatchSelf();
        }
    }
}
