using System;
using UnityEngine;

namespace PopulationCustomizer.Runtime
{
    internal sealed class PopulationUiHost : MonoBehaviour
    {
        internal Action Tick { get; set; }

        internal Action Destroying { get; set; }

        private void Update()
        {
            Tick?.Invoke();
        }

        private void OnDestroy()
        {
            Destroying?.Invoke();
            Tick = null;
            Destroying = null;
        }
    }
}
