using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EquipmentReforgeSelector
{
    internal sealed class InlineReforgeSelectorView : MonoBehaviour, IPanelStateSink
    {
        private static readonly ReforgeCandidate[] EmptyCandidates = new ReforgeCandidate[0];
        private static readonly string VanillaDeepGreen = ResolveVanillaDeepGreen();
        private static readonly KeyCode[] NumberRowKeys =
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9
        };
        private static readonly KeyCode[] NumberPadKeys =
        {
            KeyCode.Keypad1,
            KeyCode.Keypad2,
            KeyCode.Keypad3,
            KeyCode.Keypad4,
            KeyCode.Keypad5,
            KeyCode.Keypad6,
            KeyCode.Keypad7,
            KeyCode.Keypad8,
            KeyCode.Keypad9
        };
        private const string HitAreaNamePrefix = "EquipmentReforgeSelectorHitArea_";

        private Batch_ResEffect _frame;
        private InlineReforgeButton[] _ownedButtons;
        private GameObject[] _ownedHitAreas;
        private Image[] _ownedHitAreaImages;
        private ReforgeCandidate[] _candidates = EmptyCandidates;
        private bool _closing;
        private bool _buttonsDeactivated;

        public Batch_ResEffect Frame => _frame;

        public int Capacity => _frame != null && _frame.Txt_Value != null ? _frame.Txt_Value.Length : 0;

        public static InlineReforgeSelectorView Create(Batch_ResEffect frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            var view = frame.gameObject.AddComponent<InlineReforgeSelectorView>();
            view.Initialize(frame);
            return view;
        }

        public void Render(IReadOnlyList<ReforgeCandidate> candidates, ReforgeCandidate? selected)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            EnsureInitialized();
            if (candidates.Count > Capacity)
            {
                throw new InvalidOperationException("原版效果行容量不足");
            }

            _candidates = new ReforgeCandidate[candidates.Count];
            for (var index = 0; index < candidates.Count; index++)
            {
                _candidates[index] = candidates[index];
            }

            ApplyRows(selected);
        }

        public void SetSelection(ReforgeCandidate? selected)
        {
            if (_closing || _frame == null)
            {
                return;
            }

            ApplyRows(selected);
        }

        public void ShowStatus(string message, bool warning)
        {
            if (_closing || _frame == null || _frame.Txt_Value == null || _frame.Txt_Value.Length == 0)
            {
                return;
            }

            _candidates = EmptyCandidates;
            for (var index = 0; index < _frame.Txt_Value.Length; index++)
            {
                DisableButton(index);
                var text = _frame.Txt_Value[index];
                if (text == null)
                {
                    continue;
                }

                text.gameObject.SetActive(index == 0);
                if (index == 0)
                {
                    text.text = message ?? string.Empty;
                    text.color = warning ? new Color(1f, 0.45f, 0.15f) : Color.white;
                    text.raycastTarget = false;
                }
            }
        }

        public void Close()
        {
            if (_closing)
            {
                return;
            }

            _closing = true;
            DeactivateButtons();
            enabled = false;
            Destroy(this);
        }

        private void Initialize(Batch_ResEffect frame)
        {
            _frame = frame;
            var rowCount = frame.Txt_Value != null ? frame.Txt_Value.Length : 0;
            _ownedButtons = new InlineReforgeButton[rowCount];
            _ownedHitAreas = new GameObject[rowCount];
            _ownedHitAreaImages = new Image[rowCount];
        }

        private void EnsureInitialized()
        {
            if (_closing || _frame == null || _frame.Txt_Value == null || _ownedButtons == null)
            {
                throw new InvalidOperationException("原版效果列表不可用");
            }
        }

        private void ApplyRows(ReforgeCandidate? selected)
        {
            EnsureInitialized();
            var plan = InlineCandidatePlan.Create(_candidates, selected);

            for (var index = 0; index < _frame.Txt_Value.Length; index++)
            {
                var text = _frame.Txt_Value[index];
                if (text == null)
                {
                    throw new InvalidOperationException("原版效果文本行缺失");
                }

                var active = index < plan.Rows.Count;
                text.gameObject.SetActive(active);
                if (!active)
                {
                    DisableButton(index);
                    continue;
                }

                var row = plan.Rows[index];
                text.color = Color.white;
                text.raycastTarget = false;
                text.text = FormatCandidate(row.Candidate, row.IsSelected, row.CandidateIndex);
                ConfigureButton(index, text, row.CandidateIndex, row.IsSelected);
            }

            ApplyExplicitNavigation(plan.Rows.Count);
        }

        private void ConfigureButton(int slotIndex, TextMeshProUGUI text, int candidateIndex, bool selected)
        {
            var button = _ownedButtons[slotIndex];
            if (button == null)
            {
                var hitAreaName = HitAreaNamePrefix + slotIndex;
                var parent = text.rectTransform.parent;
                if (parent == null)
                {
                    throw new InvalidOperationException("原版效果行缺少父容器");
                }

                var existing = parent.Find(hitAreaName);
                GameObject hitArea;
                Image image;
                if (existing != null)
                {
                    hitArea = existing.gameObject;
                    image = hitArea.GetComponent<Image>();
                    button = hitArea.GetComponent<InlineReforgeButton>();
                    if (image == null || button == null)
                    {
                        throw new InvalidOperationException("原版效果行命中层状态不完整");
                    }
                }
                else
                {
                    hitArea = new GameObject(hitAreaName, typeof(RectTransform));
                    hitArea.transform.SetParent(parent, false);
                    var layoutElement = hitArea.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;
                    image = hitArea.AddComponent<Image>();
                    button = hitArea.AddComponent<InlineReforgeButton>();
                }

                _ownedButtons[slotIndex] = button;
                _ownedHitAreas[slotIndex] = hitArea;
                _ownedHitAreaImages[slotIndex] = image;
            }

            var ownedHitArea = _ownedHitAreas[slotIndex] ?? button.gameObject;
            var ownedImage = _ownedHitAreaImages[slotIndex] ?? ownedHitArea.GetComponent<Image>();
            _ownedHitAreas[slotIndex] = ownedHitArea;
            _ownedHitAreaImages[slotIndex] = ownedImage;
            ConfigureHitAreaLayout(text.rectTransform, (RectTransform)ownedHitArea.transform);
            ownedHitArea.SetActive(true);

            button.onClick.RemoveAllListeners();
            button.enabled = true;
            button.interactable = true;
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = ownedImage;
            ApplyButtonColors(button, ownedImage, selected);
            button.onClick.AddListener(() => RuntimeController.SelectCandidate(candidateIndex));
        }

        private static void ConfigureHitAreaLayout(RectTransform textRect, RectTransform hitAreaRect)
        {
            hitAreaRect.SetParent(textRect.parent, false);
            hitAreaRect.anchorMin = new Vector2(0f, textRect.anchorMin.y);
            hitAreaRect.anchorMax = new Vector2(1f, textRect.anchorMax.y);
            hitAreaRect.pivot = textRect.pivot;
            hitAreaRect.offsetMin = new Vector2(0f, textRect.offsetMin.y);
            hitAreaRect.offsetMax = new Vector2(0f, textRect.offsetMax.y);
            hitAreaRect.localScale = Vector3.one;
            hitAreaRect.localRotation = Quaternion.identity;
            hitAreaRect.SetSiblingIndex(textRect.GetSiblingIndex());
        }

        private static void ApplyButtonColors(InlineReforgeButton button, Image image, bool selected)
        {
            var normalColor = selected
                ? new Color(0.15f, 0.62f, 0.2f, 0.16f)
                : new Color(0f, 0f, 0f, 0.01f);
            var colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = new Color(0.18f, 0.68f, 0.24f, 0.25f);
            colors.pressedColor = new Color(0.12f, 0.55f, 0.18f, 0.32f);
            colors.selectedColor = normalColor;
            colors.disabledColor = Color.clear;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            image.raycastTarget = true;
            image.color = normalColor;
        }

        private void ApplyExplicitNavigation(int count)
        {
            var navigationPlan = CandidateNavigationPlan.Create(count);
            for (var index = 0; index < count; index++)
            {
                var row = navigationPlan.Rows[index];
                var button = _ownedButtons[index];
                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = row.UpIndex.HasValue ? _ownedButtons[row.UpIndex.Value] : null;
                navigation.selectOnDown = row.DownIndex.HasValue ? _ownedButtons[row.DownIndex.Value] : null;
                button.navigation = navigation;
            }
        }

        private void DisableButton(int index)
        {
            if (_ownedButtons == null || index < 0 || index >= _ownedButtons.Length)
            {
                return;
            }

            var button = _ownedButtons[index];
            if (button == null)
            {
                return;
            }

            button.interactable = false;
            button.enabled = false;
            button.onClick.RemoveAllListeners();
            if (_ownedHitAreas != null && index < _ownedHitAreas.Length && _ownedHitAreas[index] != null)
            {
                _ownedHitAreas[index].SetActive(false);
            }
        }

        private void DeactivateButtons()
        {
            if (_buttonsDeactivated)
            {
                return;
            }

            _buttonsDeactivated = true;
            if (_ownedButtons == null)
            {
                return;
            }

            for (var index = 0; index < _ownedButtons.Length; index++)
            {
                var button = _ownedButtons[index];
                if (button == null)
                {
                    continue;
                }

                button.interactable = false;
                button.enabled = false;
                button.onClick.RemoveAllListeners();
                if (_ownedHitAreas != null && index < _ownedHitAreas.Length && _ownedHitAreas[index] != null)
                {
                    _ownedHitAreas[index].SetActive(false);
                }

                if (_frame != null && _frame.Txt_Value != null && index < _frame.Txt_Value.Length)
                {
                    var text = _frame.Txt_Value[index];
                    if (text != null)
                    {
                        text.color = Color.white;
                        text.raycastTarget = true;
                    }
                }
            }
        }

        private static string FormatCandidate(ReforgeCandidate candidate, bool selected, int candidateIndex)
        {
            var value = Helpers.GetToolTipString((Res_Ability)candidate.AbilityId, candidate.Value) ?? string.Empty;
            var numberedValue = $"[{candidateIndex + 1}] {value}";
            return selected
                ? "<sprite name=FS_P_Right_White>" + Helpers.SetColor(VanillaDeepGreen, numberedValue + "（已选择）")
                : numberedValue;
        }

        private static string ResolveVanillaDeepGreen()
        {
            var definesType = AccessTools.TypeByName("Defines");
            var field = definesType != null ? AccessTools.Field(definesType, "Hex_DeepGreen") : null;
            return field != null && field.GetValue(null) is string value ? value : "#1E8A00";
        }

        private void Update()
        {
            if (!RuntimeController.IsViewCurrent(this, _frame))
            {
                RuntimeController.ViewDisabled(this);
                Close();
                return;
            }

            for (var index = 0; index < NumberRowKeys.Length; index++)
            {
                if (!Input.GetKeyDown(NumberRowKeys[index]) && !Input.GetKeyDown(NumberPadKeys[index]))
                {
                    continue;
                }

                if (CandidateShortcut.TryResolveDigit(index + 1, _candidates.Length, out var candidateIndex))
                {
                    RuntimeController.SelectCandidate(candidateIndex);
                }

                return;
            }
        }

        private void OnDisable()
        {
            if (_closing)
            {
                return;
            }

            _closing = true;
            DeactivateButtons();
            RuntimeController.ViewDisabled(this);
            Destroy(this);
        }

        private void OnDestroy()
        {
            DeactivateButtons();
            RuntimeController.ViewDestroyed(this);
        }
    }

    internal sealed class InlineReforgeButton : Button
    {
    }
}
