using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GodViewManagement.Runtime
{
    internal sealed class GodViewHud : IDisposable
    {
        private readonly GameObject _root;
        private readonly GameObject _settingsPanel;
        private readonly TextMeshProUGUI _modeText;
        private readonly TextMeshProUGUI _bindingText;
        private readonly TextMeshProUGUI _messageText;

        private GodViewHud(
            GameObject root,
            GameObject settingsPanel,
            TextMeshProUGUI modeText,
            TextMeshProUGUI bindingText,
            TextMeshProUGUI messageText)
        {
            _root = root;
            _settingsPanel = settingsPanel;
            _modeText = modeText;
            _bindingText = bindingText;
            _messageText = messageText;
        }

        public bool IsAlive => _root != null
                               && _settingsPanel != null
                               && _modeText != null
                               && _bindingText != null
                               && _messageText != null;

        public bool SettingsOpen => IsAlive && _settingsPanel.activeSelf;

        public bool IsCapturing { get; private set; }

        public static GodViewHud TryCreate(
            GameMgr game,
            Action openSettings,
            Action restoreDefault,
            Action hideHud,
            Action closeSettings)
        {
            var font = game?._OverView?.Txt_Name?.font;
            if (font == null)
            {
                return null;
            }

            var root = new GameObject("GodViewManagement.Hud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32700;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateButton(root.transform, "Settings", new Vector2(-420f, -16f), new Vector2(126f, 42f), font, openSettings)
                .GetComponentInChildren<TextMeshProUGUI>().text = "上帝视角设置";

            var settings = CreatePanel(root.transform, "SettingsPanel", new Vector2(600f, 320f));
            var settingsRect = settings.GetComponent<RectTransform>();
            settingsRect.anchorMin = settingsRect.anchorMax = new Vector2(0.5f, 0.5f);
            settingsRect.pivot = new Vector2(0.5f, 0.5f);
            settingsRect.anchoredPosition = Vector2.zero;

            var title = CreateText(settings.transform, "Title", font, 28f, FontStyles.Bold);
            SetRect(title.rectTransform, new Vector2(0f, 126f), new Vector2(540f, 42f));
            title.text = "上帝视角管理设置";

            var modeText = CreateText(settings.transform, "ModeStatus", font, 22f, FontStyles.Bold);
            SetRect(modeText.rectTransform, new Vector2(0f, 82f), new Vector2(540f, 34f));

            var binding = CreateText(settings.transform, "Binding", font, 22f, FontStyles.Normal);
            SetRect(binding.rectTransform, new Vector2(0f, 46f), new Vector2(540f, 34f));

            var message = CreateText(settings.transform, "Message", font, 18f, FontStyles.Normal);
            SetRect(message.rectTransform, new Vector2(0f, -2f), new Vector2(540f, 64f));
            message.color = new Color(1f, 0.76f, 0.28f, 1f);

            var rebindButton = CreateButton(settings.transform, "Rebind", Vector2.zero, new Vector2(130f, 42f), font, null);
            rebindButton.GetComponentInChildren<TextMeshProUGUI>().text = "重新绑定";
            SetRect(rebindButton.GetComponent<RectTransform>(), new Vector2(-210f, -112f), new Vector2(130f, 42f));
            var defaultButton = CreateButton(settings.transform, "Default", Vector2.zero, new Vector2(130f, 42f), font, restoreDefault);
            defaultButton.GetComponentInChildren<TextMeshProUGUI>().text = "恢复默认";
            SetRect(defaultButton.GetComponent<RectTransform>(), new Vector2(-70f, -112f), new Vector2(130f, 42f));
            var hideButton = CreateButton(settings.transform, "HideHud", Vector2.zero, new Vector2(130f, 42f), font, hideHud);
            hideButton.GetComponentInChildren<TextMeshProUGUI>().text = "隐藏 HUD";
            SetRect(hideButton.GetComponent<RectTransform>(), new Vector2(70f, -112f), new Vector2(130f, 42f));
            var closeButton = CreateButton(settings.transform, "Close", Vector2.zero, new Vector2(100f, 42f), font, closeSettings);
            closeButton.GetComponentInChildren<TextMeshProUGUI>().text = "关闭";
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(210f, -112f), new Vector2(100f, 42f));

            var hud = new GodViewHud(root, settings, modeText, binding, message);
            rebindButton.onClick.AddListener(new UnityAction(hud.BeginCapture));
            settings.SetActive(false);
            return hud;
        }

        public void ShowSettings()
        {
            if (!IsAlive)
            {
                return;
            }

            IsCapturing = false;
            _settingsPanel.SetActive(true);
        }

        public void HideSettings()
        {
            if (!IsAlive)
            {
                return;
            }

            IsCapturing = false;
            _settingsPanel.SetActive(false);
        }

        public void BeginCapture()
        {
            if (!IsAlive)
            {
                return;
            }

            IsCapturing = true;
            _messageText.text = "请按新按键；Esc 取消。修饰键不能单独绑定。";
        }

        public void EndCapture()
        {
            IsCapturing = false;
        }

        public void SetMessage(string message)
        {
            if (!IsAlive)
            {
                return;
            }

            _messageText.text = message ?? string.Empty;
        }

        public void Refresh(bool enabled, Key key, string conflict)
        {
            if (!IsAlive)
            {
                return;
            }

            _modeText.text = enabled ? "上帝视角：开" : "上帝视角：关";
            _modeText.color = enabled
                ? new Color(0.48f, 1f, 0.58f, 1f)
                : new Color(1f, 0.92f, 0.78f, 1f);
            _bindingText.text = $"当前切换键：{key}";
            if (!IsCapturing)
            {
                _messageText.text = string.IsNullOrWhiteSpace(conflict)
                    ? $"{key} 切换模式；Shift + {key} 显示或隐藏 HUD。"
                    : $"按键冲突：{conflict}。请重新绑定；Shift + {key} 仍可恢复 HUD。";
            }
        }

        public void Dispose()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = panel.GetComponent<Image>();
            image.color = new Color(0.075f, 0.09f, 0.11f, 0.96f);
            return panel;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            TMP_FontAsset font,
            Action action)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = new Color(0.16f, 0.2f, 0.24f, 0.96f);

            var button = obj.GetComponent<Button>();
            if (action != null)
            {
                button.onClick.AddListener(new UnityAction(action));
            }

            var text = CreateText(obj.transform, "Text", font, 20f, FontStyles.Bold);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.text = "上帝视角：关";
            return button;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, TMP_FontAsset font, float size, FontStyles style)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            var text = obj.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
