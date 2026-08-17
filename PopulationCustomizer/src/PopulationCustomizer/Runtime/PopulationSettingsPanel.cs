using System;
using System.Reflection;
using CasselGames.Input;
using CasselGames.UI;
using HarmonyLib;
using PopulationCustomizer.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PopulationCustomizer.Runtime
{
    internal sealed class PopulationSettingsPanel : IDisposable
    {
        private static readonly FieldInfo FilterButtonField =
            AccessTools.Field(typeof(StatisticsCitizenListUI), "_filterBtn");
        private static readonly FieldInfo SearchButtonField =
            AccessTools.Field(typeof(StatisticsCitizenListUI), "_searchBtn");

        private readonly StatisticsCitizenListUI _listUi;
        private readonly ApplySettingsDelegate _apply;
        private readonly RestoreSettingsDelegate _restore;
        private readonly Action<PopulationSettingsPanel> _destroyed;
        private GameObject _entryButton;
        private GameObject _overlay;
        private Toggle _citizenToggle;
        private Toggle _ratronToggle;
        private TMP_InputField _citizenInput;
        private TMP_InputField _ratronInput;
        private TextMeshProUGUI _citizenStatus;
        private TextMeshProUGUI _ratronStatus;
        private TextMeshProUGUI _message;
        private string _previousActionMap;
        private bool _ownsActionMap;
        private bool _refreshingControls;
        private bool _disposed;

        private PopulationSettingsPanel(
            StatisticsCitizenListUI listUi,
            ApplySettingsDelegate apply,
            RestoreSettingsDelegate restore,
            Action<PopulationSettingsPanel> destroyed)
        {
            _listUi = listUi;
            _apply = apply;
            _restore = restore;
            _destroyed = destroyed;
        }

        internal static PopulationSettingsPanel TryCreate(
            StatisticsCitizenListUI listUi,
            ApplySettingsDelegate apply,
            RestoreSettingsDelegate restore,
            Action<PopulationSettingsPanel> destroyed)
        {
            if (listUi == null || FilterButtonField == null || SearchButtonField == null)
            {
                return null;
            }

            var filterButton = FilterButtonField.GetValue(listUi) as Button;
            var searchButton = SearchButtonField.GetValue(listUi) as Button;
            var parent = filterButton?.transform?.parent;
            var font = GameMgr.Instance?._EcoMgr?.m_CitizenUI?.Txt_Num?.font ??
                       listUi.GetComponentInChildren<TextMeshProUGUI>(true)?.font;
            if (filterButton == null || searchButton == null || parent == null ||
                searchButton.transform.parent != parent || font == null)
            {
                return null;
            }

            var panel = new PopulationSettingsPanel(listUi, apply, restore, destroyed);
            panel.Build(font, filterButton, searchButton);
            return panel;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            RestoreInputMap();
            var entry = _entryButton;
            var overlay = _overlay;
            _entryButton = null;
            _overlay = null;
            if (entry != null)
            {
                UnityEngine.Object.Destroy(entry);
            }

            if (overlay != null)
            {
                var host = overlay.GetComponent<PopulationUiHost>();
                if (host != null)
                {
                    host.Destroying = null;
                }

                UnityEngine.Object.Destroy(overlay);
            }
        }

        private void Build(TMP_FontAsset font, Button filterButton, Button searchButton)
        {
            CreateStatisticsEntry(font, filterButton, searchButton);

            _overlay = new GameObject(
                "PopulationCustomizer.Overlay",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(PopulationUiHost));
            _overlay.transform.SetParent(null, false);
            var canvas = _overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32720;
            var scaler = _overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var blocker = CreateImage(_overlay.transform, "Blocker", new Color(0f, 0f, 0f, 0.55f));
            Stretch(blocker.rectTransform);

            var body = CreateImage(_overlay.transform, "Panel", new Color(0.075f, 0.09f, 0.11f, 0.98f));
            SetRect(body.rectTransform, Vector2.zero, new Vector2(720f, 470f));

            var title = CreateText(body.transform, "Title", font, 30f, FontStyles.Bold);
            title.text = "人口自定义";
            SetRect(title.rectTransform, new Vector2(0f, 190f), new Vector2(640f, 44f));

            var warning = CreateText(body.transform, "Warning", font, 17f, FontStyles.Normal);
            warning.text = "编辑数值会自动勾选自定义；较高人口会增加 CPU、寻路与存档负担。";
            warning.color = new Color(1f, 0.78f, 0.35f, 1f);
            SetRect(warning.rectTransform, new Vector2(0f, 150f), new Vector2(650f, 34f));

            CreateRow(body.transform, font, "鼠民", 75f, out _citizenToggle, out _citizenInput, out _citizenStatus);
            CreateRow(body.transform, font, "机器鼠", -15f, out _ratronToggle, out _ratronInput, out _ratronStatus);
            _citizenInput.onValueChanged.AddListener(new UnityAction<string>(HandleCitizenInputChanged));
            _ratronInput.onValueChanged.AddListener(new UnityAction<string>(HandleRatronInputChanged));

            _message = CreateText(body.transform, "Message", font, 18f, FontStyles.Normal);
            _message.color = new Color(0.55f, 0.9f, 1f, 1f);
            SetRect(_message.rectTransform, new Vector2(0f, -92f), new Vector2(650f, 50f));

            var applyButton = CreateButton(body.transform, "Apply", font, ApplyFromInputs);
            applyButton.GetComponentInChildren<TextMeshProUGUI>().text = "应用到当前存档";
            SetRect(applyButton.GetComponent<RectTransform>(), new Vector2(-190f, -175f), new Vector2(200f, 46f));
            var restoreButton = CreateButton(body.transform, "Restore", font, RestoreVanilla);
            restoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "恢复原版";
            SetRect(restoreButton.GetComponent<RectTransform>(), new Vector2(30f, -175f), new Vector2(160f, 46f));
            var closeButton = CreateButton(body.transform, "Close", font, Hide);
            closeButton.GetComponentInChildren<TextMeshProUGUI>().text = "关闭";
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(220f, -175f), new Vector2(120f, 46f));

            var host = _overlay.GetComponent<PopulationUiHost>();
            host.Tick = Tick;
            host.Destroying = HandleUnityDestroy;
            _overlay.SetActive(false);
        }

        private void CreateStatisticsEntry(TMP_FontAsset font, Button filterButton, Button searchButton)
        {
            var parent = filterButton.transform.parent;
            var filterRect = filterButton.GetComponent<RectTransform>();
            var searchRect = searchButton.GetComponent<RectTransform>();
            var filterPosition = filterRect.anchoredPosition;
            var searchPosition = searchRect.anchoredPosition;
            var horizontalStep = Mathf.Abs(filterPosition.x - searchPosition.x);
            if (horizontalStep < 1f)
            {
                horizontalStep = Mathf.Max(filterRect.rect.width, searchRect.rect.width) + 12f;
            }

            var entryObject = UnityEngine.Object.Instantiate(filterButton.gameObject, parent, false);
            entryObject.name = "PopulationCustomizer.Entry";
            _entryButton = entryObject;

            var entryRect = entryObject.GetComponent<RectTransform>();
            entryRect.anchorMin = searchRect.anchorMin;
            entryRect.anchorMax = searchRect.anchorMax;
            entryRect.pivot = searchRect.pivot;
            entryRect.anchoredPosition = new Vector2(searchPosition.x - horizontalStep, searchPosition.y);

            var button = entryObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            var rootGraphic = button.targetGraphic;
            foreach (var graphic in entryObject.GetComponentsInChildren<Graphic>(true))
            {
                if (!ReferenceEquals(graphic, rootGraphic))
                {
                    graphic.enabled = false;
                }
            }

            var label = CreateText(entryObject.transform, "Text", font, 18f, FontStyles.Bold);
            label.text = "上限";
            Stretch(label.rectTransform);
            button.onClick.AddListener(new UnityAction(Show));

            entryObject.transform.SetSiblingIndex(searchButton.transform.GetSiblingIndex());
            if (parent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
        }

        private void Show()
        {
            if (_disposed || _overlay == null)
            {
                return;
            }

            CaptureInputMap();
            RefreshControls(resetInputs: true);
            _message.text = string.Empty;
            _overlay.SetActive(true);
        }

        private void Hide()
        {
            if (_overlay != null)
            {
                _overlay.SetActive(false);
            }

            RestoreInputMap();
        }

        private void Tick()
        {
            if (_overlay != null && _overlay.activeSelf && InputMgr.GetEscapeButtonDown())
            {
                Hide();
            }
        }

        private void ApplyFromInputs()
        {
            if (!LimitRules.TryParse(_citizenInput.text, out var citizenLimit))
            {
                _message.text = "鼠民上限必须是 0–999 的整数。";
                return;
            }

            if (!LimitRules.TryParse(_ratronInput.text, out var ratronLimit))
            {
                _message.text = "机器鼠上限必须是 0–999 的整数。";
                return;
            }

            var settings = new LimitSettings(
                _citizenToggle.isOn,
                citizenLimit,
                _ratronToggle.isOn,
                ratronLimit);
            if (_apply(settings, out var message))
            {
                RefreshControls(resetInputs: false);
            }

            _message.text = message;
        }

        private void RestoreVanilla()
        {
            if (_restore(out var message))
            {
                RefreshControls(resetInputs: true);
            }

            _message.text = message;
        }

        private void HandleCitizenInputChanged(string value)
        {
            if (!_refreshingControls && _citizenToggle != null)
            {
                _citizenToggle.isOn = true;
            }
        }

        private void HandleRatronInputChanged(string value)
        {
            if (!_refreshingControls && _ratronToggle != null)
            {
                _ratronToggle.isOn = true;
            }
        }

        private void RefreshControls(bool resetInputs)
        {
            var game = GameMgr.Instance;
            if (game?._ProsperityUI == null || game._SysMgr == null || game._T_UnitMgr == null)
            {
                _message.text = "当前存档尚未准备完成。";
                return;
            }

            var effectiveCitizen = game._ProsperityUI.GetMaxCitizenCount();
            var effectiveRatron = game._SysMgr.GetGBotMaxCount();
            var vanillaCitizen = LimitRuntime.LastVanillaCitizenLimit;
            var vanillaRatron = LimitRuntime.LastVanillaRatronLimit;
            var citizenCount = game._T_UnitMgr.List_Citizen?.Count ?? 0;
            var ratronCount = game._T_UnitMgr.List_GBot?.Count ?? 0;
            var current = LimitRuntime.Current;

            _refreshingControls = true;
            try
            {
                _citizenToggle.isOn = current.CitizenEnabled;
                _ratronToggle.isOn = current.RatronEnabled;
                if (resetInputs)
                {
                    _citizenInput.text = (current.CitizenEnabled ? current.CitizenLimit : ClampForInput(vanillaCitizen)).ToString();
                    _ratronInput.text = (current.RatronEnabled ? current.RatronLimit : ClampForInput(vanillaRatron)).ToString();
                }
            }
            finally
            {
                _refreshingControls = false;
            }

            _citizenStatus.text = FormatStatus(citizenCount, vanillaCitizen, effectiveCitizen);
            _ratronStatus.text = FormatStatus(ratronCount, vanillaRatron, effectiveRatron);
            _citizenStatus.color = citizenCount > effectiveCitizen ? new Color(1f, 0.48f, 0.42f, 1f) : Color.white;
            _ratronStatus.color = ratronCount > effectiveRatron ? new Color(1f, 0.48f, 0.42f, 1f) : Color.white;
        }

        private void CaptureInputMap()
        {
            var input = InputMgr.Instance;
            if (input == null || _ownsActionMap)
            {
                return;
            }

            _previousActionMap = input.NowActionMapKey;
            input.SetActionMap(InputMgr.INPUT_ACTIONMAP_UI);
            _ownsActionMap = true;
        }

        private void RestoreInputMap()
        {
            if (!_ownsActionMap)
            {
                return;
            }

            var input = InputMgr.Instance;
            if (input != null && input.NowActionMapKey == InputMgr.INPUT_ACTIONMAP_UI)
            {
                if (string.IsNullOrWhiteSpace(_previousActionMap))
                {
                    input.SetDefaultActionMap();
                }
                else
                {
                    input.SetActionMap(_previousActionMap);
                }
            }

            _ownsActionMap = false;
            _previousActionMap = null;
        }

        private void HandleUnityDestroy()
        {
            RestoreInputMap();
            _entryButton = null;
            _overlay = null;
            _disposed = true;
            _destroyed?.Invoke(this);
        }

        private static string FormatStatus(int current, int vanilla, int effective)
        {
            var suffix = current > effective ? "（已超额，将停止新增）" : string.Empty;
            return $"当前 {current}　原版 {vanilla}　生效 {effective}{suffix}";
        }

        private static int ClampForInput(int value)
        {
            return Mathf.Clamp(value, LimitRules.Minimum, LimitRules.Maximum);
        }

        private static void CreateRow(
            Transform parent,
            TMP_FontAsset font,
            string label,
            float y,
            out Toggle toggle,
            out TMP_InputField input,
            out TextMeshProUGUI status)
        {
            var name = CreateText(parent, label + "Label", font, 23f, FontStyles.Bold);
            name.text = label;
            name.alignment = TextAlignmentOptions.MidlineLeft;
            SetRect(name.rectTransform, new Vector2(-265f, y + 18f), new Vector2(100f, 40f));

            toggle = CreateToggle(parent, label + "Toggle", font);
            toggle.GetComponentInChildren<TextMeshProUGUI>().text = "自定义";
            SetRect(toggle.GetComponent<RectTransform>(), new Vector2(-130f, y + 18f), new Vector2(130f, 38f));

            input = CreateInput(parent, label + "Input", font);
            SetRect(input.GetComponent<RectTransform>(), new Vector2(30f, y + 18f), new Vector2(130f, 42f));

            status = CreateText(parent, label + "Status", font, 18f, FontStyles.Normal);
            status.alignment = TextAlignmentOptions.MidlineLeft;
            SetRect(status.rectTransform, new Vector2(120f, y - 24f), new Vector2(470f, 34f));
        }

        private static Button CreateButton(Transform parent, string name, TMP_FontAsset font, Action action)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<Image>().color = new Color(0.16f, 0.24f, 0.3f, 0.98f);
            var button = obj.GetComponent<Button>();
            if (action != null)
            {
                button.onClick.AddListener(new UnityAction(action));
            }

            var text = CreateText(obj.transform, "Text", font, 19f, FontStyles.Bold);
            Stretch(text.rectTransform);
            return button;
        }

        private static Toggle CreateToggle(Transform parent, string name, TMP_FontAsset font)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            obj.transform.SetParent(parent, false);
            var box = CreateImage(obj.transform, "Background", new Color(0.16f, 0.2f, 0.24f, 1f));
            SetRect(box.rectTransform, new Vector2(-46f, 0f), new Vector2(30f, 30f));
            var check = CreateImage(box.transform, "Checkmark", new Color(0.32f, 0.85f, 0.48f, 1f));
            SetRect(check.rectTransform, Vector2.zero, new Vector2(20f, 20f));
            var label = CreateText(obj.transform, "Label", font, 18f, FontStyles.Normal);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            SetRect(label.rectTransform, new Vector2(25f, 0f), new Vector2(80f, 34f));
            var toggle = obj.GetComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            return toggle;
        }

        private static TMP_InputField CreateInput(Transform parent, string name, TMP_FontAsset font)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<Image>().color = new Color(0.05f, 0.065f, 0.08f, 1f);
            var viewport = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(obj.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect);
            viewportRect.offsetMin = new Vector2(10f, 3f);
            viewportRect.offsetMax = new Vector2(-10f, -3f);
            var text = CreateText(viewport.transform, "Text", font, 22f, FontStyles.Normal);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            Stretch(text.rectTransform);
            var placeholder = CreateText(viewport.transform, "Placeholder", font, 20f, FontStyles.Italic);
            placeholder.text = "0-999";
            placeholder.color = new Color(1f, 1f, 1f, 0.35f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            Stretch(placeholder.rectTransform);
            var input = obj.GetComponent<TMP_InputField>();
            input.textViewport = viewportRect;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.characterLimit = 4;
            return input;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var image = obj.GetComponent<Image>();
            image.color = color;
            return image;
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
