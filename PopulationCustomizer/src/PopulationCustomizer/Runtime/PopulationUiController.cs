using System;
using BepInEx.Logging;
using CasselGames.UI;
using PopulationCustomizer.Core;

namespace PopulationCustomizer.Runtime
{
    internal delegate bool ApplySettingsDelegate(LimitSettings settings, out string message);

    internal delegate bool RestoreSettingsDelegate(out string message);

    internal sealed class PopulationUiController : IDisposable
    {
        private readonly ManualLogSource _logger;
        private readonly ApplySettingsDelegate _apply;
        private readonly RestoreSettingsDelegate _restore;
        private StatisticsCitizenListUI _listUi;
        private PopulationSettingsPanel _panel;

        internal PopulationUiController(
            ManualLogSource logger,
            ApplySettingsDelegate apply,
            RestoreSettingsDelegate restore)
        {
            _logger = logger;
            _apply = apply;
            _restore = restore;
        }

        internal void Attach(StatisticsCitizenListUI listUi)
        {
            if (listUi == null)
            {
                return;
            }

            if (ReferenceEquals(_listUi, listUi) && _panel != null)
            {
                return;
            }

            ResetSession();
            _listUi = listUi;
            _panel = PopulationSettingsPanel.TryCreate(listUi, _apply, _restore, HandlePanelDestroyed);
            if (_panel == null)
            {
                _logger.LogWarning("鼠民名单标题栏尚未准备好，无法创建人口上限按钮；重新进入存档后会再次尝试。");
                return;
            }

            _logger.LogInfo("鼠民名单放大镜左侧的“上限”按钮已创建。");
        }

        internal void ResetSession()
        {
            _panel?.Dispose();
            _panel = null;
            _listUi = null;
        }

        public void Dispose()
        {
            ResetSession();
        }

        private void HandlePanelDestroyed(PopulationSettingsPanel panel)
        {
            if (ReferenceEquals(_panel, panel))
            {
                _panel = null;
                _listUi = null;
            }
        }
    }
}
