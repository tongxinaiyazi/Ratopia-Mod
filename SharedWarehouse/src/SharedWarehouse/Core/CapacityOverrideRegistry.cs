using System;
using System.Collections.Generic;

namespace SharedWarehouse.Core
{
    internal sealed class CapacityOverrideRegistry<TTarget>
        where TTarget : class
    {
        private readonly Func<TTarget, float> _getter;
        private readonly Action<TTarget, float> _setter;
        private readonly float _overrideValue;
        private readonly Func<float, bool> _isOverrideValue;
        private readonly Dictionary<TTarget, float> _originals =
            new Dictionary<TTarget, float>(ReferenceEqualityComparer<TTarget>.Instance);

        public CapacityOverrideRegistry(
            Func<TTarget, float> getter,
            Action<TTarget, float> setter,
            float overrideValue,
            Func<float, bool> isOverrideValue)
        {
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
            _overrideValue = overrideValue;
            _isOverrideValue = isOverrideValue ?? throw new ArgumentNullException(nameof(isOverrideValue));
        }

        public void Apply(TTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!_originals.ContainsKey(target))
            {
                _originals.Add(target, _getter(target));
            }

            _setter(target, _overrideValue);
        }

        public void RestoreAll()
        {
            foreach (var pair in _originals)
            {
                if (_isOverrideValue(_getter(pair.Key)))
                {
                    _setter(pair.Key, pair.Value);
                }
            }

            _originals.Clear();
        }
    }
}
