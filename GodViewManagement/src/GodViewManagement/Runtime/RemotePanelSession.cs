using CasselGames.Input;
using UnityEngine;

namespace GodViewManagement.Runtime
{
    internal sealed class RemotePanelSession
    {
        private readonly RemotePanelState _state = new RemotePanelState();
        private int _generation;

        public bool IsOpen => _state.IsOpen;

        public Building Target => _state.Target as Building;

        public bool ShouldBlockQueenAction(BuildMidUI panel)
        {
            var target = Target;
            return target != null
                && panel != null
                && ReferenceEquals(panel.m_BuildInfoUI, target.m_BuildInfoUI)
                && _state.ShouldBlockQueenAction(panel, target);
        }

        public void Open(BuildMidUI panel, Building building, bool keepGodViewInput)
        {
            var generation = ++_generation;
            _state.Open(panel, building);
            try
            {
                panel.BuildMid_Open(building.m_BuildInfoUI, () => Close(generation, keepGodViewInput));
                HideQueenActionSlots(panel);
            }
            catch
            {
                _state.Clear();
                RestoreInput(keepGodViewInput);
                throw;
            }
        }

        public void Tick(BuildMidUI panel)
        {
            if (!IsOpen)
            {
                return;
            }

            if (Target == null || panel == null)
            {
                _state.Clear();
                return;
            }

            HideQueenActionSlots(panel);
        }

        public void Clear()
        {
            _generation++;
            _state.Clear();
        }

        private void Close(int generation, bool keepGodViewInput)
        {
            if (generation != _generation)
            {
                return;
            }

            _generation++;
            _state.Clear();
            RestoreInput(keepGodViewInput);
        }

        private static void RestoreInput(bool keepGodViewInput)
        {
            if (keepGodViewInput)
            {
                InputMgr.Instance?.SetActionMap(InputMgr.INPUT_ACTIONMAP_UI);
            }
            else
            {
                InputMgr.Instance?.SetDefaultActionMap();
            }
        }

        private static void HideQueenActionSlots(BuildMidUI panel)
        {
            SetInactive(panel.m_QueenSlot?.Obj);
            SetInactive(panel.m_QueenSlot2?.Obj);
            SetInactive(panel.m_QueenSlot3?.Obj);
            SetInactive(panel.m_QueenSlot4?.Obj);
            SetInactive(panel.m_QueenSlot5?.Obj);
        }

        private static void SetInactive(GameObject value)
        {
            if (value != null && value.activeSelf)
            {
                value.SetActive(false);
            }
        }
    }
}
