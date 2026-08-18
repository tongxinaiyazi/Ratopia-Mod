using System;
using System.Collections.Generic;

namespace EquipmentReforgeSelector
{
    public sealed class ScopedListReferenceOverride<TAbility, TValue> : IDisposable
    {
        private readonly Action<IList<TAbility>> _setAbilities;
        private readonly Action<IList<TValue>> _setValues;
        private readonly IList<TAbility> _originalAbilities;
        private readonly IList<TValue> _originalValues;
        private bool _disposed;

        public ScopedListReferenceOverride(
            Func<IList<TAbility>> getAbilities,
            Action<IList<TAbility>> setAbilities,
            Func<IList<TValue>> getValues,
            Action<IList<TValue>> setValues,
            TAbility ability,
            TValue value)
        {
            if (getAbilities == null)
            {
                throw new ArgumentNullException(nameof(getAbilities));
            }

            if (setAbilities == null)
            {
                throw new ArgumentNullException(nameof(setAbilities));
            }

            if (getValues == null)
            {
                throw new ArgumentNullException(nameof(getValues));
            }

            if (setValues == null)
            {
                throw new ArgumentNullException(nameof(setValues));
            }

            _setAbilities = setAbilities;
            _setValues = setValues;
            _originalAbilities = getAbilities();
            _originalValues = getValues();

            var replacementAbilities = new List<TAbility> { ability };
            var replacementValues = new List<TValue> { value };
            setAbilities(replacementAbilities);
            setValues(replacementValues);
            IsApplied = ReferenceEquals(getAbilities(), replacementAbilities) &&
                        ReferenceEquals(getValues(), replacementValues);

            if (!IsApplied)
            {
                RestoreOriginalReferences();
            }
        }

        public bool IsApplied { get; private set; }

        public void Dispose()
        {
            if (_disposed || !IsApplied)
            {
                return;
            }

            RestoreOriginalReferences();
            _disposed = true;
        }

        private void RestoreOriginalReferences()
        {
            _setAbilities(_originalAbilities);
            _setValues(_originalValues);
        }
    }
}
