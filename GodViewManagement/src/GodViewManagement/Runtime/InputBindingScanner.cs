using CasselGames.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GodViewManagement.Runtime
{
    internal sealed class InputBindingScanner
    {
        public string FindConflict(Key key)
        {
            var keyboard = Keyboard.current;
            var input = InputMgr.Instance;
            if (keyboard == null || input == null)
            {
                return null;
            }

            var control = keyboard[key];
            var asset = input.GetNowInputActionAsset(InputMgr.ControlScheme, false);
            if (control == null || asset == null)
            {
                return null;
            }

            foreach (var map in asset.actionMaps)
            {
                foreach (var binding in map.bindings)
                {
                    var path = binding.effectivePath;
                    if (string.IsNullOrWhiteSpace(path) || !InputControlPath.Matches(path, control))
                    {
                        continue;
                    }

                    var actionName = string.IsNullOrWhiteSpace(binding.action) ? "未命名动作" : binding.action;
                    return $"{map.name}/{actionName}";
                }
            }

            return null;
        }

        public bool TryGetPressedKey(out Key key)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                foreach (KeyControl control in keyboard.allKeys)
                {
                    if (control.wasPressedThisFrame)
                    {
                        key = control.keyCode;
                        return true;
                    }
                }
            }

            key = Key.None;
            return false;
        }
    }
}
