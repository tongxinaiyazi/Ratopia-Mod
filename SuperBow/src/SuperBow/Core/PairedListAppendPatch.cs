using System;
using System.Collections.Generic;

namespace SuperBow.Core
{
    public sealed class PairedListAppendPatch<TAbility> : IDisposable
    {
        private readonly IList<TAbility> _abilities;
        private readonly IList<float> _values;
        private readonly TAbility _ability;
        private readonly float _value;
        private readonly int _index;
        private bool _disposed;

        private PairedListAppendPatch(
            IList<TAbility> abilities,
            IList<float> values,
            TAbility ability,
            float value,
            int index)
        {
            _abilities = abilities;
            _values = values;
            _ability = ability;
            _value = value;
            _index = index;
        }

        public static bool TryApply(
            IList<TAbility> abilities,
            IList<float> values,
            TAbility ability,
            float value,
            out PairedListAppendPatch<TAbility> patch)
        {
            patch = null;
            if (abilities == null || values == null || abilities.Count != values.Count)
            {
                return false;
            }

            var comparer = EqualityComparer<TAbility>.Default;
            for (var index = 0; index < abilities.Count; index++)
            {
                if (comparer.Equals(abilities[index], ability))
                {
                    return false;
                }
            }

            var insertionIndex = abilities.Count;
            try
            {
                abilities.Add(ability);
                try
                {
                    values.Add(value);
                }
                catch
                {
                    if (abilities.Count > insertionIndex &&
                        comparer.Equals(abilities[insertionIndex], ability))
                    {
                        abilities.RemoveAt(insertionIndex);
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }

            patch = new PairedListAppendPatch<TAbility>(
                abilities,
                values,
                ability,
                value,
                insertionIndex);
            return true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_abilities.Count <= _index || _values.Count <= _index)
            {
                return;
            }

            if (!EqualityComparer<TAbility>.Default.Equals(_abilities[_index], _ability) ||
                !_values[_index].Equals(_value))
            {
                return;
            }

            _values.RemoveAt(_index);
            _abilities.RemoveAt(_index);
        }
    }
}
